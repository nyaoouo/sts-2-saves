using System;
using System.Linq;
using System.Text;
using MegaCrit.Sts2.Core.Saves;
using MegaCrit.Sts2.Core.Saves.Runs;
using NyMod.Saves.Features.SaveArchive.Models;

namespace NyMod.Saves.Features.SaveArchive.Logic;

internal sealed class SaveArchiveSummaryFactory
{
	public SaveArchiveSummary Create(byte[] payloadBytes)
	{
		try
		{
			string json = Encoding.UTF8.GetString(payloadBytes);
			ReadSaveResult<SerializableRun> result = JsonSerializationUtility.FromJson<SerializableRun>(json);
			if (!result.Success || result.SaveData == null)
			{
				return new SaveArchiveSummary
				{
					SummaryAvailable = false,
					ErrorMessage = result.ErrorMessage ?? result.Status.ToString()
				};
			}

			SerializableRun save = result.SaveData;
			SerializablePlayer? firstPlayer = save.Players.FirstOrDefault();
			return new SaveArchiveSummary
			{
				SummaryAvailable = true,
				CharacterIds = save.Players
					.Select(static player => player.CharacterId?.ToString() ?? "unknown")
					.ToList(),
				PlayerCount = save.Players.Count,
				SaveTime = save.SaveTime,
				StartTime = save.StartTime,
				RunTime = save.RunTime,
				Ascension = save.Ascension,
				CurrentActIndex = save.CurrentActIndex,
				EstimatedFloor = save.VisitedMapCoords.Count,
				CurrentHp = firstPlayer?.CurrentHp,
				MaxHp = firstPlayer?.MaxHp,
				Gold = firstPlayer?.Gold,
				PlatformType = save.PlatformType.ToString(),
				DailyTime = save.DailyTime
			};
		}
		catch (Exception ex)
		{
			return new SaveArchiveSummary
			{
				SummaryAvailable = false,
				ErrorMessage = ex.Message
			};
		}
	}
}