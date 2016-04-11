package no.hesa.veiviserenuitnarvik;

import android.app.Activity;
import android.app.ListActivity;
import android.app.SearchManager;
import android.content.Intent;
import android.os.Bundle;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;

import no.hesa.veiviserenuitnarvik.api.ActionInterface;
import no.hesa.veiviserenuitnarvik.api.Api;

public class SearchResultsActivity extends ListActivity implements ActionInterface{

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_search_results);
        handleIntent(getIntent());

    }

    @Override
    protected void onNewIntent(Intent intent) {
        setIntent(intent);
        handleIntent(intent);
    }

    @Override
    public void onListItemClick(ListView listView, View view, int position, long id) {
        //Add some code here..
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
        try {
            ArrayList<String> arrayList = new ArrayList<>();
            JSONArray jsonArray = jsonObject.getJSONArray("locations");
            for (int i = 0; i < jsonArray.length(); i++) {
                JSONObject jObject = jsonArray.getJSONObject(i);
                arrayList.add(jObject.getString("name"));
            }
            ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(this,android.R.layout.simple_list_item_1,arrayList);
            ListView listView = (ListView) findViewById(R.id.listview);
            listView.setAdapter(arrayAdapter);
        }
        catch (Exception ex) {
            ex.printStackTrace();
        }
    }
}
