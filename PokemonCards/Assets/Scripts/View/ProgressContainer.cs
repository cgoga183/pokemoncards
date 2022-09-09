using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PockemonCards.View
{
    public class ProgressContainer : MonoBehaviour
    {
        //used to display any notifications
        public TextMeshProUGUI notificationText;
        public TextMeshProUGUI progessBarCounterText;
        public Slider progressBar;

        //the color used to display notificatdions for the fail state
        public Color failedColor = Color.red;
        //the color used to display notificatdions for the succesful state
        public Color successColor = Color.green;
        //the color used to display notificatdions for the neutral state
        public Color neutralColor = Color.white;

        //counter used to keep track of completed items. used by the the progress
        private int _completedItemsCount = 0;
        //the maximum size of the items waiting to be completed.used by the progress bar
        private int _maxItemsSize;

        /// <summary>
        /// The enum holding all possible states of this container
        /// </summary>
        public enum ProgressStates
        {
            STATE_NOTHING_TO_FETCH = 1,
            STATE_FETCHING = 2,
            STATE_RETRY_FAILED_FETCH = 3,
            STATE_FETCHED_SUCCESFULLY = 4,
            STATE_FINISH_FETCH_WITH_FAILED = 5,
            STATE_FINISH_FETCH_WITH_UNKOWN_ERROR = 6
        }

        /// <summary>
        /// It will set the state of the container
        /// </summary>
        /// <param name="state">The state we want the container to be set to</param>
        /// <param name="noOfFailedFetches">The number of failed requests. This is optional</param>
        public void SetState(ProgressStates state, int noOfFailedFetches = 0)
        {
            switch (state)
            {
                case ProgressStates.STATE_NOTHING_TO_FETCH:
                    OnStateNothingToFetch();
                    break;
                case ProgressStates.STATE_FETCHING:
                    OnStateFetching();
                    break;
                case ProgressStates.STATE_RETRY_FAILED_FETCH:
                    OnStateRetryFailedFetch(noOfFailedFetches);
                    break;
                case ProgressStates.STATE_FETCHED_SUCCESFULLY:
                    OnStateFetchedSuccesfully();
                    break;
                case ProgressStates.STATE_FINISH_FETCH_WITH_FAILED:
                    OnStateFinishedWithFailed(noOfFailedFetches);
                    break;
                case ProgressStates.STATE_FINISH_FETCH_WITH_UNKOWN_ERROR:
                    OnStateFinishFetchWithUnknownError();
                    break;
            }
        }

        /// <summary>
        /// Will reset the progress bar and set the maximum items size
        /// </summary>
        /// <param name="maxItemsSize"></param>
        public void ResetProgressBar(int maxItemsSize)
        {
            progressBar.value = 0;
            _completedItemsCount = 0;
            _maxItemsSize = maxItemsSize;
        }

        /// <summary>
        /// Will update the progress bar and the text with the latest completed items count
        /// </summary>
        public void UpdateProgressBar()
        {
            _completedItemsCount++;
            progressBar.value = (float)_completedItemsCount / (float)_maxItemsSize;
            progessBarCounterText.text = string.Format(Texts.FETCH_PER_TOTAL, _completedItemsCount, _maxItemsSize);
        }

        private void OnStateNothingToFetch()
        {
            notificationText.color = failedColor;
            notificationText.text = Texts.YOU_DONT_OWN_POKEMONS;
        }

        private void OnStateFetching()
        {
            notificationText.color = neutralColor;
            notificationText.text = Texts.FETCHING_POKEMONS;
        }

        private void OnStateRetryFailedFetch(int noOfFailedFetches)
        {
            notificationText.color = failedColor;
            notificationText.text = string.Format(Texts.RETRY_FAILED_FETCHES, noOfFailedFetches);
        }

        private void OnStateFetchedSuccesfully()
        {
            notificationText.color = successColor;
            notificationText.text = Texts.FETCHES_SUCCESFULLY;
        }

        private void OnStateFinishedWithFailed(int noOfFailedFetches)
        {
            notificationText.color = failedColor;
            notificationText.text = string.Format(Texts.FETECHED_WITH_ERRORS, noOfFailedFetches);
        }

        private void OnStateFinishFetchWithUnknownError()
        {
            notificationText.color = failedColor;
            notificationText.text = Texts.FETCHED_WITH_UNKNOWN_ERROR;
        }


    }
}