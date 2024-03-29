﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;

namespace NexNux.Models;
public class GameList
{
    public GameList(string gameListFile)
    {
        GameListFile = gameListFile;
        Games = LoadList();
    }

    public string GameListFile { get; set; }
    public List<Game> Games { get; set; }

    public void SaveList()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(GameListFile) ?? throw new InvalidOperationException());
            using FileStream createStream = File.Create(GameListFile);
            JsonSerializer.Serialize(createStream, Games, new JsonSerializerOptions() { WriteIndented = true });
            createStream.Dispose();

        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
        }
    }

    public List<Game> LoadList()
    {
        List<Game> loadedGames = new List<Game>();
        try
        {
            string jsonString = File.ReadAllText(GameListFile);
            loadedGames = JsonSerializer.Deserialize<List<Game>>(jsonString) ?? throw new InvalidOperationException();
        }
        catch (FileNotFoundException ex)
        {
            Debug.WriteLine(ex);
            Games = new List<Game>(); //Make sure we don't write null to a JSON file
            SaveList();
            LoadList();
        }
        catch (DirectoryNotFoundException ex)
        {
            Debug.WriteLine(ex);
            Games = new List<Game>(); //Make sure we don't write null to a JSON file
            SaveList();
            LoadList();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }

        return loadedGames;
    }

    public void ModifyGame(string gameName, GameType gameType, string deployDir, string modDir, string? appDataDir)
    {
        // Maybe there should be a try-catch within this?
        if (DirsInUse(deployDir, modDir))
            throw new Exception("Directories already in use");
        Game game = new Game(gameName, gameType, deployDir, modDir, appDataDir);
        Games.Remove(game); // This does not currently use the RemoveGame method because that would delete everything,
                            // not sure if modifying game dirs should be supported at the minute
        Games.Add(game);
        SaveList();
        
    }
    public void RemoveGame(Game game)
    {
        try
        {
            Game? existingGame = Games.Find(item => item.GameName == game.GameName);
            while (existingGame != null)
            {
                existingGame.DeleteMods();
                Games.Remove(existingGame);
                existingGame = Games.Find(item => item.GameName == game.GameName);
            }
            SaveList();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
            throw;
        }
    }

    public bool DirsInUse(string deployDir, string modDir)
    {
        return Games.Find(item => item.DeployDirectory == deployDir) != null ||
               Games.Find(item => item.ModsDirectory == modDir) != null;
    }
}