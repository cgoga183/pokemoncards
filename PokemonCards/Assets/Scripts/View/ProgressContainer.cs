using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProgressContainer : MonoBehaviour
{
    public TextMeshProUGUI titleAndErrors;
    public Slider progressBar;
   
    public Color failedColor = Color.red;
    public Color successColor = Color.green;
    public Color neutralColor = Color.white;

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
    
    private void OnStateNothingToFetch()
    {
        titleAndErrors.color = failedColor;
        titleAndErrors.text = "You don't own any pokemons";
    }

    private void OnStateFetching()
    {
        titleAndErrors.color = neutralColor;
        titleAndErrors.text = "Fetching pokemons ...";
    }

    private void OnStateRetryFailedFetch(int noOfFailedFetches)
    {
        progressBar.value = 0;
        titleAndErrors.color = failedColor;
        titleAndErrors.text = "There were " + noOfFailedFetches + " failed requests. Trying to fetch them again";
    }

    private void OnStateFetchedSuccesfully()
    {
        titleAndErrors.color = successColor;
        titleAndErrors.text = "Fetched succesfully all pokemons";
    }

    private void OnStateFinishedWithFailed(int noOfFailedFetches)
    {
        titleAndErrors.color = failedColor;
        titleAndErrors.text = "Fetched with errors. We are missing " + noOfFailedFetches + " pokemons";
    }

    private void OnStateFinishFetchWithUnknownError()
    {
        titleAndErrors.color = failedColor;
        titleAndErrors.text = "Could not fetch pokemons. Please try again.";
    }

    public void UpdateProgress(int fetchedItemsCount, int totalItemsCount)
    {
        progressBar.value = (float)fetchedItemsCount / (float)totalItemsCount;
    }
}
