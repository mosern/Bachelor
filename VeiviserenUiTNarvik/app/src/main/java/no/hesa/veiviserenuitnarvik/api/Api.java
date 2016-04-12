package no.hesa.veiviserenuitnarvik.api;

import android.content.Context;
import android.content.res.Resources;
import android.util.Pair;

import java.util.ArrayList;
import java.util.List;

import no.hesa.veiviserenuitnarvik.R;

/**
 * Created by thasimon on 10.04.2016.
 */
public class Api {
    //All actionstrings
    public static final String ALL_USERS = "LOAD_ALL_USERS";
    public static final String DO_SEARCH = "DO_SEARCH";

    private ActionInterface actionInterface;
    private Resources res;

    /**
     * Initiate the API repository
     * @param actionInterface The interface that does the callback after the asynctask is done calling the API
     * @param res A resource identifier to get all the strings associated with the API
     */
    public Api(ActionInterface actionInterface,Resources res) {
        this.actionInterface = actionInterface;
        this.res = res;
    }

    /**
     * Returns all users in the API
     */
    public void allUsers() {
        String url = res.getString(R.string.api_users);
        List<Pair<String,String>> params = new ArrayList<>();
        callAsyncTask(url,params,ALL_USERS,"GET");
    }

    /**
     * Do a search with the specified callstring
     * @param searchString The string that is used for searching
     */
    public void doSearch(String searchString) {
        String url = res.getString(R.string.api_locations);
        List<Pair<String,String>> params = new ArrayList<>();
        params.add(new Pair<String, String>("search",searchString));
        callAsyncTask(url,params,DO_SEARCH,"GET");
    }

    /**
     * Calls the async task to connect with the API
     * @param url The url in the API that is to be requested
     * @param params Parameters to include in the request, input a list with no entries if there is no parameters
     * @param actionString The string that is used in the callback to identify the action
     * @param dataType Method of parameters (GET or POST)
     */
    private void callAsyncTask(String url, List<Pair<String,String>> params,String actionString,String dataType) {
        ApiAsyncTask asyncTask = new ApiAsyncTask(actionInterface,actionString,url,dataType);
        asyncTask.execute(params);
    }
}
