package no.hesa.veiviserenuitnarvik;

import android.app.Activity;
import android.app.ProgressDialog;
import android.app.SearchManager;
import android.content.Intent;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.MotionEvent;
import android.widget.ExpandableListView;
import android.widget.Toast;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import com.google.gson.Gson;


//import no.hesa.veiviserenuitnarvik.api.ActionInterface;
//import no.hesa.veiviserenuitnarvik.api.Api;
import no.hesa.positionlibrary.api.ActionInterface;
import no.hesa.positionlibrary.api.Api;
import no.hesa.veiviserenuitnarvik.dataclasses.Person;

public class SearchResultsActivity extends Activity implements ActionInterface, SimpleGestureFilter.SimpleGestureListener{

    // http://www.androidhive.info/2013/07/android-expandable-list-view-tutorial/


    private JSONArray jsonArray = null;

    ExpandableListAdapter listAdapter;
    ExpandableListView expListView;
    List<String> listDataHeader;
    HashMap<String, List<? extends Object>> listDataChild;

    private SimpleGestureFilter detector;
    private ProgressDialog progressDialog;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //overridePendingTransition(R.anim.slide_in, R.anim.slide_out);
        setContentView(R.layout.activity_search_results);

        // Detect touched area
        detector = new SimpleGestureFilter(this,this);

        handleIntent(getIntent());

        expListView = (ExpandableListView) findViewById(R.id.exlv_search_results);
    }

    @Override
    protected void onNewIntent(Intent intent) {
        setIntent(intent);
        handleIntent(intent);
    }

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

    private void handleIntent(Intent intent) {
        if (Intent.ACTION_SEARCH.equals(intent.getAction())) {
            String query = intent.getStringExtra(SearchManager.QUERY);
            Api api = new Api(this,getResources());
            api.doSearch(query);
        }
    }

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
                        jsonArray = jsonObject.getJSONArray("locations");

                        for (int i = 0; i < jsonArray.length(); i++) { // iterates both arrays within Locations-array
                            if (jsonArray.getJSONArray(i) != null) {
                                for (int j = 0; j < jsonArray.getJSONArray(i).length(); j++) { // iterates the contents of each array, so the objects within are available
                                    if (jsonArray.getJSONArray(i).length() != 0) {
                                        JSONObject jObject = jsonArray.getJSONArray(i).getJSONObject(j);

                                        listDataHeader.add(jObject.getString("name"));
                                        countDataHeaderInsertions++;

                                        if (i == 0) //first array, contains locations
                                        {
                                            List<no.hesa.veiviserenuitnarvik.dataclasses.Location> objDetails = new ArrayList<>();
                                            Gson gson = new Gson();
                                            no.hesa.veiviserenuitnarvik.dataclasses.Location obj = gson.fromJson(jObject.toString(), no.hesa.veiviserenuitnarvik.dataclasses.Location.class); // convert
                                            objDetails.add(obj);
                                            listDataChild.put(listDataHeader.get(countDataHeaderInsertions - 1), objDetails);
                                            orderOfClassTypes.add("location");
                                        }

                                        if (i == 1) // second array, contains persons
                                        {
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
                        Toast.makeText(getApplicationContext(), "No results, please try another search", Toast.LENGTH_LONG).show();
                        this.finish();
                    }
                    listAdapter = new ExpandableListAdapter(this, listDataHeader, listDataChild, orderOfClassTypes);
                    expListView.setAdapter(listAdapter);// setting list adapter
                    startExtvListeners();

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
        //Do something here..
    }

    @Override
    public boolean dispatchTouchEvent(MotionEvent me) {
        // Call onTouchEvent of SimpleGestureFilter class
        this.detector.onTouchEvent(me);
        return super.dispatchTouchEvent(me);
    }

    @Override
    public void onSwipe(int direction) {
        String str = "";

        switch (direction) {

            case SimpleGestureFilter.SWIPE_RIGHT : str = "Swipe Right";
                //Toast.makeText(this, str, Toast.LENGTH_SHORT).show();
                finish();
                break;
            case SimpleGestureFilter.SWIPE_LEFT :  str = "Swipe Left";
                finish();
                break;
            case SimpleGestureFilter.SWIPE_DOWN :  str = "Swipe Down";

                break;
            case SimpleGestureFilter.SWIPE_UP :    str = "Swipe Up";

                break;

        }
      //  Toast.makeText(this, str, Toast.LENGTH_SHORT).show();
    }

    @Override
    public void onDoubleTap() {
        // Toast.makeText(this, "Double Tap", Toast.LENGTH_SHORT).show();
    }
}
