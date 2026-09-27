using ProjectTD.Core;
using UnityEngine;
using UnityEngine.UI;

namespace ProjectTD.UI
{
    /// <summary>Shows lives, currency, the current wave, and VICTORY/DEFEAT when the game ends.</summary>
    public class Hud : MonoBehaviour
    {
        [SerializeField] GameController game;
        [SerializeField] Text livesText;
        [SerializeField] Text currencyText;
        [SerializeField] Text waveText;
        [SerializeField] Text outcomeText;

        int shownLives = -1;
        int shownCurrency = -1;
        int shownWave = -1;
        GameOutcome shownOutcome = (GameOutcome)(-1);

        void Update()
        {
            GameSession session = game.Session;

            if (session.Lives != shownLives)
            {
                shownLives = session.Lives;
                livesText.text = $"Lives: {shownLives}";
            }

            if (session.Currency != shownCurrency)
            {
                shownCurrency = session.Currency;
                currencyText.text = $"Currency: {shownCurrency}";
            }

            int wave = game.Waves.CurrentWaveNumber;
            if (wave != shownWave)
            {
                shownWave = wave;
                waveText.text = wave == 0 ? $"Wave: - / {game.Waves.WaveCount}" : $"Wave: {wave} / {game.Waves.WaveCount}";
            }

            if (session.Outcome != shownOutcome)
            {
                shownOutcome = session.Outcome;
                outcomeText.gameObject.SetActive(shownOutcome != GameOutcome.InProgress);
                outcomeText.text = shownOutcome == GameOutcome.Victory ? "VICTORY" : "DEFEAT";
                outcomeText.color = shownOutcome == GameOutcome.Victory ? new Color(1f, 0.85f, 0.2f) : new Color(1f, 0.3f, 0.3f);
            }
        }
    }
}
