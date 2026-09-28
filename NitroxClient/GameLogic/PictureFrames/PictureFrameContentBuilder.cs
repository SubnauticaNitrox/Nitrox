using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace NitroxClient.GameLogic.PictureFrames;

/// <summary>
/// Morphs a local screenshot file into a fresh, size-capped, metadata-stripped JPEG produced by Unity.
/// </summary>
public static class PictureFrameContentBuilder
{
    private const int MinJpegQuality = 20;
    private const int JpegQualityStep = 15;

    public readonly struct Result
    {
        public bool Success { get; private init; }
        public Texture2D? Texture { get; private init; }
        public byte[]? JpegBytes { get; private init; }
        public string? ContentHash { get; private init; }
        public string? ErrorMessage { get; private init; }

        public static Result Fail(string errorMessage) => new() { Success = false, ErrorMessage = errorMessage };

        public static Result Ok(Texture2D texture, byte[] jpegBytes, string contentHash) =>
            new() { Success = true, Texture = texture, JpegBytes = jpegBytes, ContentHash = contentHash };
    }
    
    public static Result TryBuild(string filePath, int maxDimension, int jpegQuality, int maxBytes)
    {
        byte[] rawBytes;
        try
        {
            rawBytes = File.ReadAllBytes(filePath);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            return Result.Fail($"Could not read file: {ex.Message}");
        }

        Texture2D loaded = new(2, 2, TextureFormat.RGBA32, false);
        if (!loaded.LoadImage(rawBytes, false))
        {
            UnityEngine.Object.Destroy(loaded);
            return Result.Fail("File is not a valid, decodable image.");
        }

        Texture2D final = ResizeIfNeeded(loaded, maxDimension);
        if (!ReferenceEquals(final, loaded))
        {
            UnityEngine.Object.Destroy(loaded);
        }

        int quality = jpegQuality;
        byte[] jpegBytes = final.EncodeToJPG(quality);
        while (jpegBytes.Length > maxBytes && quality > MinJpegQuality)
        {
            quality = Math.Max(MinJpegQuality, quality - JpegQualityStep);
            jpegBytes = final.EncodeToJPG(quality);
        }

        if (jpegBytes.Length > maxBytes)
        {
            UnityEngine.Object.Destroy(final);
            return Result.Fail($"Picture is too large to sync ({jpegBytes.Length} bytes, max {maxBytes}) even at reduced quality.");
        }

        string contentHash = ComputeHashHex(jpegBytes);
        return Result.Ok(final, jpegBytes, contentHash);
    }
    
    private static string ComputeHashHex(byte[] bytes)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] hash = sha256.ComputeHash(bytes);
        StringBuilder builder = new(hash.Length * 2);
        foreach (byte b in hash)
        {
            builder.Append(b.ToString("x2"));
        }
        return builder.ToString();
    }
    
    private static Texture2D ResizeIfNeeded(Texture2D source, int maxDimension)
    {
        int width = source.width;
        int height = source.height;
        if (width <= maxDimension && height <= maxDimension)
        {
            return source;
        }

        float scale = maxDimension / (float)Math.Max(width, height);
        int targetWidth = Math.Max(1, Mathf.RoundToInt(width * scale));
        int targetHeight = Math.Max(1, Mathf.RoundToInt(height * scale));

        RenderTexture renderTexture = RenderTexture.GetTemporary(targetWidth, targetHeight, 0, RenderTextureFormat.ARGB32);
        RenderTexture previousActive = RenderTexture.active;
        try
        {
            Graphics.Blit(source, renderTexture);
            RenderTexture.active = renderTexture;

            Texture2D resized = new(targetWidth, targetHeight, TextureFormat.RGBA32, false);
            resized.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
            resized.Apply();
            return resized;
        }
        finally
        {
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(renderTexture);
        }
    }
}
