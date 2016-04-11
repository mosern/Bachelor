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
    public Api(ActionInterface actionInterface,Resources res) {
        this.actionInterface = actionInterface;
        this.res = res;
    }
    public void allUsers() {
        String url = res.getString(R.string.api_users);
        List<Pair<String,String>> params = new ArrayList<>();
        callAsyncTask(url,params,ALL_USERS,"GET");
    }

    public void doSearch(String searchString) {
        String url = res.getString(R.string.api_locations);
        List<Pair<String,String>> params = new ArrayList<>();
        params.add(new Pair<String, String>("search",searchString));
        callAsyncTask(url,params,DO_SEARCH,"GET");
    }

    private void callAsyncTask(String url, List<Pair<String,String>> params,String actionString,String dataType) {
        ApiAsyncTask asyncTask = new ApiAsyncTask(actionInterface,actionString,url,dataType);
        asyncTask.execute(params);
    }
}
