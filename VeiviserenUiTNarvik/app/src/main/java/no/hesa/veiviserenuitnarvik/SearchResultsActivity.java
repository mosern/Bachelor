package no.hesa.veiviserenuitnarvik;

import android.app.Activity;
import android.app.SearchManager;
import android.content.Context;
import android.content.Intent;
import android.os.Bundle;

import android.support.v7.app.AppCompatActivity;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ExpandableListView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import com.google.gson.Gson;

import no.hesa.positionlibrary.api.ActionInterface;
import no.hesa.positionlibrary.api.Api;
import no.hesa.veiviserenuitnarvik.dataclasses.Person;

/**
 * This activity is used with a supplied search string from the starting activity.
 * It then uses the Api-class to access the search function on HESA Application Server (HESA AS).
 * The returned result is then iterated through and returned to it's initial dataclass-form
 * (Location, Person, Type, Coordinate) before it then uses ExpandableListAdapter to show an
 * expandable listview filled with data from Locations and Persons.
 *
 * Swiping detection is supported through the SimpleGestureFilter-class.
 *
 */

public class SearchResultsActivity extends AppCompatActivity implements ActionInterface, SimpleGestureFilter.SimpleGestureListener{

    private JSONArray jsonArray = null;

    // ExpandableListView variables
    private ExpandableListAdapter listAdapter;
    private ExpandableListView expListView;
    private List<String> listDataHeader;
    private HashMap<String, List<? extends Object>> listDataChild;

    private SimpleGestureFilter detector; // gesture detector
    private ProgressBar search_spinner; // loading spinner

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //overridePendingTransition(R.anim.slide_in, R.anim.slide_out);
        setContentView(R.layout.activity_search_results);


        detector = new SimpleGestureFilter(this,this); // gesture detector
        search_spinner = (ProgressBar)findViewById(R.id.search_progress_bar);
        handleIntent(getIntent()); // handle passed intent
        expListView = (ExpandableListView) findViewById(R.id.exlv_search_results);
    }

    @Override
    protected void onNewIntent(Intent intent) {
        setIntent(intent);
        handleIntent(intent);
    }

    // NOT USED: But not removed due to possible future use
    private void startExtvListeners() {
/*
        // Listview on child click listener
        expListView.setOnChildClickListener(new ExpandableListView.OnChildClickListener() {

            @Override
            public boolean onChildClick(ExpandableListView parent, View v,
                                        int groupPosition, int childPosition, long id) {
                Toast.makeText(
                        getApplicationContext(),
                        listDataHeader.get(groupPosition)
                                + " : "
                                + listDataChild.get(
                                listDataHeader.get(groupPosition)).get(
                                childPosition), Toast.LENGTH_SHORT)
                        .show();
                return false;
            }
        });

        // Listview Group collasped listener
        expListView.setOnGroupCollapseListener(new ExpandableListView.OnGroupCollapseListener() {

            @Override
            public void onGroupCollapse(int groupPosition) {
                Toast.makeText(getApplicationContext(),
                        listDataHeader.get(groupPosition) + " Collapsed",
                        Toast.LENGTH_SHORT).show();

            }
        });

        */
    }

    /**
     * Handles received search query by requesting all matching search result from HESA AS
     *
     * @param intent
     */
    private void handleIntent(Intent intent) {
        if (Intent.ACTION_SEARCH.equals(intent.getAction())) {
            String query = intent.getStringExtra(SearchManager.QUERY);
            Api api = new Api(this);
            search_spinner.setVisibility(View.VISIBLE); // shows loading spinner, indicating that the API is generating search results
            api.doSearch(query);
        }
    }

    /**
     * Handles returned data from HESA AS, converts it back into dataclasses (Person, Location, Coordinate, Type).
     * Adds said classes to lists of Person and Location objects, which are then used for setup of
     * an expandable list.
     *
     * @param jsonObject
     * @param actionString
     */
    @Override
    public void onCompletedAction(JSONObject jsonObject, String actionString) {
        switch (actionString) {
            case Api.DO_SEARCH:
                try {
                    listDataHeader = new ArrayList<String>();
                    int countDataHeaderInsertions = 0;
                    listDataChild = new HashMap<String, List<? extends Object>>();
                    List<String> orderOfClassTypes = new ArrayList<>();
                    if (jsonObject != null) {
                        jsonArray = jsonObject.getJSONArray("results");

                        for (int i = 0; i < jsonArray.length(); i++) { // iterates both arrays within Locations-array
                            if (jsonArray.getJSONArray(i) != null) {
                                for (int j = 0; j < jsonArray.getJSONArray(i).length(); j++) { // iterates the contents of each array, so the objects within are available
                                    if (jsonArray.getJSONArray(i).length() != 0) {
                                        JSONObject jObject = jsonArray.getJSONArray(i).getJSONObject(j);

                                        listDataHeader.add(jObject.getString("name"));
                                        countDataHeaderInsertions++;

                                        if (i == 0) { //first array, contains locations
                                            List<no.hesa.veiviserenuitnarvik.dataclasses.Location> objDetails = new ArrayList<>();
                                            Gson gson = new Gson();
                                            no.hesa.veiviserenuitnarvik.dataclasses.Location obj = gson.fromJson(jObject.toString(), no.hesa.veiviserenuitnarvik.dataclasses.Location.class); // convert
                                            objDetails.add(obj);
                                            listDataChild.put(listDataHeader.get(countDataHeaderInsertions - 1), objDetails);
                                            orderOfClassTypes.add("location");
                                        }

                                        if (i == 1) { // second array, contains persons
                                            List<Person> objDetails = new ArrayList<>();
                                            Gson gson = new Gson();
                                            Person obj = gson.fromJson(jObject.toString(), Person.class); // convert
                                            objDetails.add(obj);
                                            listDataChild.put(listDataHeader.get(countDataHeaderInsertions - 1), objDetails);
                                            orderOfClassTypes.add("person");
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        showCustomToast(getApplicationContext(), getResources().getString(R.string.search_no_results), Toast.LENGTH_LONG);
                        this.finish();
                    }

                    search_spinner.setVisibility(View.GONE);
                    listAdapter = new ExpandableListAdapter(this, listDataHeader, listDataChild, orderOfClassTypes);
                    expListView.setAdapter(listAdapter);// setting list adapter
                    startExtvListeners();

                    // autoexpands few results
                    if (countDataHeaderInsertions <= 3) {
                        for (int i = 0; i < 3; i++) {
                            expListView.expandGroup(i);
                        }
                    }
                }
                catch (Exception ex) {
                    ex.printStackTrace();
                }
                break;
            default:
                break;
        }
    }

    @Override
    public void onAuthorizationFailed() {

    }

    /**
     * Handles touch events via SimpleGestureFilter-class
     * @param me motionevent
     * @return touch event
     */
    @Override
    public boolean dispatchTouchEvent(MotionEvent me) {
        // Call onTouchEvent of SimpleGestureFilter class
        this.detector.onTouchEvent(me);
        return super.dispatchTouchEvent(me);
    }

    /**
     * Handles swipe events
     * @param direction direction of the swipe
     */
    @Override
    public void onSwipe(int direction) {
        switch (direction) {
            case SimpleGestureFilter.SWIPE_RIGHT :
                finish();
                break;
            case SimpleGestureFilter.SWIPE_LEFT :
                finish();
                break;
            case SimpleGestureFilter.SWIPE_DOWN :
                break;
            case SimpleGestureFilter.SWIPE_UP :
                break;
        }
    }

    /**
     * Handles double taps
     */
    @Override
    public void onDoubleTap() {
        // Toast.makeText(this, "Double Tap", Toast.LENGTH_SHORT).show();
    }

    /**
     * Shows a custom toast, using toast.xml
     * @param context Context
     * @param msg The toast message
     * @param length Duration, use Toast.LENGTH_SHORT or LENGTH_LONG
     */
    private void showCustomToast(Context context, String msg, int length)
    {
        LayoutInflater inflater = getLayoutInflater();
        View toastLayout = inflater.inflate(R.layout.toast, (ViewGroup) findViewById(R.id.toast_layout));

        TextView text = (TextView) toastLayout.findViewById(R.id.toast_content);
        text.setText(msg);

        Toast t = new Toast(getApplicationContext());
        t.setView(toastLayout);
        t.setDuration(length);
        t.show();
    }
}
