package no.hesa.veiviserenuitnarvik;

import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.os.Bundle;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.Snackbar;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.View;
import android.widget.TextView;

import no.hesa.positionlibrary.PositionLibrary;
import no.hesa.positionlibrary.dijkstra.exception.PathNotFoundException;

public class MeasurementActivity extends AppCompatActivity {

    private PositionLibrary positionLibrary = null;

    private final BroadcastReceiver outputReceiver = new BroadcastReceiver() {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (intent.getAction().equals("no.hesa.positionlibrary.Output")) {
                TextView textView = (TextView) findViewById(R.id.tv_returned_position);
                //textView.setText(intent.getStringExtra("DistanceOutput"));
                double[] position = intent.getDoubleArrayExtra("position");
                //textView.setText(intent.getDoubleArrayExtra("position").toString());
            }
        }
    };

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_measurement);
        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab);
        fab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Snackbar.make(view, "Replace with your own action", Snackbar.LENGTH_LONG)
                        .setAction("Action", null).show();
            }
        });

        getSupportActionBar().setDisplayHomeAsUpEnabled(true);
        positionLibrary = new PositionLibrary();
        positionLibrary.wifiPosition.registerBroadcast(this);
        registerReceiver(outputReceiver, new IntentFilter("no.hesa.positionlibrary.Output"));
    }

    @Override
    protected void onStop() {
        super.onStop();
        if (positionLibrary != null) {
            positionLibrary.wifiPosition.unRegisterBroadcast(this);
        }
        unregisterReceiver(outputReceiver);
    }
}
