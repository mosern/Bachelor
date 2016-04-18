package no.hesa.veiviserenuitnarvik;

import android.app.Activity;
import android.app.ListActivity;
import android.app.SearchManager;
import android.content.Intent;
import android.location.Location;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ExpandableListView;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.gms.maps.model.LatLng;

import org.json.JSONArray;
import org.json.JSONObject;

import java.lang.reflect.Type;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import com.google.gson.Gson;


import me.imid.swipebacklayout.lib.SwipeBackLayout;
import me.imid.swipebacklayout.lib.app.SwipeBackActivity;
import no.hesa.veiviserenuitnarvik.api.ActionInterface;
import no.hesa.veiviserenuitnarvik.api.Api;
import no.hesa.veiviserenuitnarvik.dataclasses.Person;

public class SearchResultsActivity extends SwipeBackActivity  implements ActionInterface{

    // http://www.androidhive.info/2013/07/android-expandable-list-view-tutorial/
    // https://github.com/ikew0ng/SwipeBackLayout

    private JSONArray jsonArray = null;
    TextView txtView;
    // tutorial variables
    ExpandableListAdapter listAdapter;
    ExpandableListView expListView;
    List<String> listDataHeader;
    HashMap<String, List<? extends Object>> listDataChild;
    private SwipeBackLayout mSwipeBackLayout;


    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        // overridePendingTransition(R.anim.slide_in, R.anim.slide_out); // doesn't work for transparent activity
        setContentView(R.layout.activity_search_results);

        handleIntent(getIntent());
        // txtView = (TextView) findViewById(R.id.tv_arraylistout);
        // get the listview
        expListView = (ExpandableListView) findViewById(R.id.exlv_search_results);
        mSwipeBackLayout = getSwipeBackLayout();
        mSwipeBackLayout.setEdgeTrackingEnabled(mSwipeBackLayout.EDGE_LEFT);

        mSwipeBackLayout.addSwipeListener(new SwipeBackLayout.SwipeListener() {
            @Override
            public void onScrollStateChange(int state, float scrollPercent) {
                Toast.makeText(getApplicationContext(), "1!", Toast.LENGTH_SHORT).show();
            }

            @Override
            public void onEdgeTouch(int edgeFlag) {
                Toast.makeText(getApplicationContext(), "2!", Toast.LENGTH_SHORT).show();
            }

            @Override
            public void onScrollOverThreshold() {
                Toast.makeText(getApplicationContext(), "3!", Toast.LENGTH_SHORT).show();
            }
        });

    }

    @Override
    protected void onNewIntent(Intent intent) {
        setIntent(intent);
        handleIntent(intent);
    }
/*
    @Override
    public void onListItemClick(ListView listView, View view, int position, long id) {
        if (jsonArray != null) {
            if (jsonArray.length() >= position) {
               try {

                   //jsonArray.getJSONArray(i).getJSONObject(j);
                    JSONObject jsonObject = jsonArray.getJSONObject(position);
                    Intent intent = new Intent(this,MapActivity.class);
                    intent.setAction("LAT_LNG_RETURN");
                    intent.putExtra("lat",jsonObject.getJSONObject("coordinate").getDouble("lat"));
                    intent.putExtra("lng",jsonObject.getJSONObject("coordinate").getDouble("lng"));
                    startActivity(intent);

                }
                catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        }
    }
*/
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

                    jsonArray = jsonObject.getJSONArray("locations");

                    for (int i = 0; i < jsonArray.length(); i++) { // iterates both arrays within Locations-array
                        if (jsonArray.getJSONArray(i) != null) {
                            for (int j = 0; j < jsonArray.getJSONArray(i).length(); j++) { // iterates the contents of each array, so the objects within are available
                                if (jsonArray.getJSONArray(i).length() != 0) {
                                    JSONObject jObject = jsonArray.getJSONArray(i).getJSONObject(j);

                                    listDataHeader.add(jObject.getString("name"));
                                    countDataHeaderInsertions++;
                                    
                                    if (i == 0) //first array, contains rooms
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

                //    txtView.setText(jsonObject.toString()); // text printout av array for testing

                //    ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(this,android.R.layout.simple_list_item_1,arrayList);
                //    setListAdapter(arrayAdapter);

                    listAdapter = new ExpandableListAdapter(this, listDataHeader, listDataChild, orderOfClassTypes);

                    // setting list adapter
                    expListView.setAdapter(listAdapter);

                    startExtvListeners();

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

}
