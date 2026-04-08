using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using NyMod.Saves.Features.SaveArchive.Models;

namespace NyMod.Saves.Features.SaveArchive.Logic;

internal sealed class RunIdentityService
{
	public string ResolveRunId(SaveArchiveSummary summary, byte[] payloadBytes, bool isMultiplayer)
	{
		string mode = isMultiplayer ? "mp" : "sp";
		if (summary.SummaryAvailable)
		{
			string characters = string.Join(",", summary.CharacterIds.OrderBy(static id => id, StringComparer.Ordinal));
			string daily = summary.DailyTime?.ToUnixTimeSeconds().ToString() ?? "none";
			string stableKey = string.Join("|", mode, summary.StartTime, summary.PlatformType ?? string.Empty, summary.GameMode ?? string.Empty, daily, characters, summary.PlayerCount);
			return "run_" + ComputeShortHash(Encoding.UTF8.GetBytes(stableKey));
		}

		return "run_" + ComputeShortHash(payloadBytes);
	}

	private static string ComputeShortHash(byte[] bytes)
	{
		byte[] hash = SHA256.HashData(bytes);
		return Convert.ToHexString(hash[..8]).ToLowerInvariant();
	}
}