using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Threading.Tasks;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using System;
using nbaplayers.Models;

public class NBAPlayerService
{
    private readonly AppDbContext _context;

    public NBAPlayerService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> UploadAndSavePlayersAsync(Stream pdfStream)
    {
        var playersAdded = 0;
        
        // Copy the stream to a MemoryStream to avoid synchronous read issues
        using var memoryStream = new MemoryStream();
        await pdfStream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        
        using var reader = new PdfReader(memoryStream);
        using var pdfDoc = new PdfDocument(reader);
        
        for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
        {
            var page = pdfDoc.GetPage(i);
            var text = PdfTextExtractor.GetTextFromPage(page);
            
            // Log the extracted text for debugging
            Console.WriteLine($"Extracted text from page {i}: {text}");
            
            // Skip empty pages
            if (string.IsNullOrWhiteSpace(text))
                continue;
                
            try
            {
                var players = ParsePlayerData(text);
                foreach (var player in players)
                {
                    _context.NBAPlayers.Add(player);
                    playersAdded++;
                    Console.WriteLine($"Player added: {player.FirstName} {player.LastName}");
                }
            }
            catch (Exception ex)
            {
                // Log parsing error but continue with other pages
                Console.WriteLine($"Error parsing page {i}: {ex.Message}");
            }
        }

        if (playersAdded > 0)
        {
            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Successfully saved {playersAdded} players to database");
            }
            catch (Exception saveEx)
            {
                Console.WriteLine($"Error saving to database: {saveEx.Message}");
                if (saveEx.InnerException != null)
                {
                    Console.WriteLine($"Inner exception: {saveEx.InnerException.Message}");
                }
                throw; // Re-throw to maintain the error flow
            }
        }
        
        return playersAdded;
    }

    public async Task<List<NBAPlayer>> GetAllPlayersAsync()
    {
        return await _context.NBAPlayers.ToListAsync();
    }

    public async Task<int> AddPlayerAsync(NBAPlayer player)
    {
        _context.NBAPlayers.Add(player);
        return await _context.SaveChangesAsync();
    }

    private List<NBAPlayer> ParsePlayerData(string text)
    {
        var players = new List<NBAPlayer>();
        var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            var trimmedLine = line.Trim();
            
            // Skip header lines or empty lines
            if (string.IsNullOrEmpty(trimmedLine) || 
                trimmedLine.Contains("First Name") || 
                trimmedLine.Contains("Last Name") ||
                trimmedLine.Contains("Date of Birth"))
            {
                continue;
            }
            
            // Try to parse this line as player data
            var playerData = ParsePlayerLine(trimmedLine);
            if (playerData != null)
            {
                players.Add(playerData); // Collect valid players
            }
        }
        
        return players; // Return the list of players
    }

    private NBAPlayer? ParsePlayerLine(string line)
    {
        try
        {
            // Pattern: FirstName LastName YYYY-MM-DD TeamName Yes/No Yes/No
            // We'll use regex to extract the date first, then work backwards and forwards
            
            var datePattern = @"\d{4}-\d{2}-\d{2}";
            var dateMatch = System.Text.RegularExpressions.Regex.Match(line, datePattern);
            
            if (!dateMatch.Success)
            {
                Console.WriteLine($"No date found in line: {line}");
                return null;
            }
            
            var dateOfBirthStr = dateMatch.Value;
            var dateIndex = dateMatch.Index;
            
            // Extract name part (everything before the date)
            var namePart = line.Substring(0, dateIndex).Trim();
            var nameWords = namePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (nameWords.Length < 2)
            {
                Console.WriteLine($"Insufficient name data in line: {line}");
                return null;
            }
            
            var firstName = nameWords[0];
            var lastName = nameWords[1];
            
            // Extract everything after the date
            var afterDatePart = line.Substring(dateIndex + dateOfBirthStr.Length).Trim();
            var afterDateWords = afterDatePart.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (afterDateWords.Length < 3)
            {
                Console.WriteLine($"Insufficient data after date in line: {line}");
                return null;
            }
            
            // Last two words should be retired and injured status
            var retiredStr = afterDateWords[afterDateWords.Length - 2];
            var injuredStr = afterDateWords[afterDateWords.Length - 1];
            
            // Everything else is the team name
            var teamWords = afterDateWords.Take(afterDateWords.Length - 2);
            var team = string.Join(" ", teamWords);

            // Validate required fields
            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName))
            {
                Console.WriteLine("Missing required player name data");
                return null;
            }

            // Parse date of birth
            if (!DateTime.TryParse(dateOfBirthStr, out var dateOfBirth))
            {
                Console.WriteLine($"Invalid date format: {dateOfBirthStr}");
                return null;
            }

            // Parse boolean values with fallback
            var retired = ParseBoolean(retiredStr);
            var injured = ParseBoolean(injuredStr);

            Console.WriteLine($"Parsed: {firstName} {lastName}, {dateOfBirthStr}, {team}, Retired: {retiredStr}, Injured: {injuredStr}");

            return new NBAPlayer(
                firstName: firstName,
                lastName: lastName,
                dateOfBirth: dateOfBirth,
                team: team ?? "Unknown",
                retired: retired,
                injured: injured
            );
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing player line: {ex.Message}");
            return null;
        }
    }

    private bool ParseBoolean(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return false;
            
        value = value.Trim().ToLower();
        return value == "true" || value == "yes" || value == "1" || value == "retired" || value == "injured";
    }
}
