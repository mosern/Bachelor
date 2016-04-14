package no.hesa.veiviserenuitnarvik;

import android.app.Activity;
import android.app.ListActivity;
import android.app.SearchManager;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;
import android.widget.TextView;
import android.widget.Toast;

import com.google.android.gms.maps.model.LatLng;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;

import no.hesa.veiviserenuitnarvik.api.ActionInterface;
import no.hesa.veiviserenuitnarvik.api.Api;

public class SearchResultsActivity extends ListActivity implements ActionInterface{

    private JSONArray jsonArray = null;
    TextView txtView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_search_results);
        handleIntent(getIntent());
        txtView = (TextView) findViewById(R.id.tv_arraylistout);

    }

    @Override
    protected void onNewIntent(Intent intent) {
        setIntent(intent);
        handleIntent(intent);
    }

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

    private void handleIntent(Intent intent) {
        if (Intent.ACTION_SEARCH.equals(intent.getAction())) {
            String query = intent.getStringExtra(SearchManager.QUERY);
            Api api = new Api(this,getResources());
            api.doSearch(query);
        }
    }

    @Override
    public void onCompletedAction(JSONObject jsonObject, String actionString) {

        /*
       {
  "locations": [
                    [
                      {
                        "id": 2,
                        "coordinate":
                        {
                          "id": 1,
                          "lng": 68.436265,
                          "lat": 17.433672,
                          "alt": 1.0
                        },
                        "type":
                        {
                          "id": 1,
                          "name": "TestType"
                        },
                        "name": "TestLoc",
                        "locNr": "A1111",
                        "hits": 11
                      },
         */


        switch (actionString) {
            case Api.DO_SEARCH:
                try {
                    ArrayList<String> arrayList = new ArrayList<>();
                    jsonArray = jsonObject.getJSONArray("locations");



                    for (int i = 0; i < jsonArray.length(); i++) {
                        if (jsonArray.getJSONArray(i) != null) {
                            for (int j = 0; j < jsonArray.getJSONArray(i).length(); j++) {
                                if (jsonArray.getJSONArray(i).length() != 0) {
                                    JSONObject jObject = jsonArray.getJSONArray(i).getJSONObject(j);
                                    arrayList.add(jObject.getString("name"));
                                }
                            }
                        }
                    }

                    txtView.setText(jsonObject.toString()); // text printout av array for testing

                    ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(this,android.R.layout.simple_list_item_1,arrayList);
                    setListAdapter(arrayAdapter);

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
