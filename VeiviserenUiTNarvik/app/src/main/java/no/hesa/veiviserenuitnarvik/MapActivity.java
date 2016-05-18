package no.hesa.veiviserenuitnarvik;

import android.annotation.TargetApi;
import android.app.SearchManager;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.graphics.drawable.Drawable;
import android.os.Build;
import android.os.Bundle;
import android.os.Looper;
import android.support.design.widget.FloatingActionButton;
import android.support.v4.content.ContextCompat;
import android.support.v4.view.MenuItemCompat;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.SearchView;
import android.support.v7.widget.Toolbar;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.Menu;
import android.view.MenuItem;
import android.view.View;
import android.view.ViewGroup;
import android.view.Window;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;


import com.google.android.gms.maps.CameraUpdateFactory;
import com.google.android.gms.maps.GoogleMap;
import com.google.android.gms.maps.OnMapReadyCallback;
import com.google.android.gms.maps.SupportMapFragment;
import com.google.android.gms.maps.model.BitmapDescriptor;
import com.google.android.gms.maps.model.BitmapDescriptorFactory;
import com.google.android.gms.maps.model.Circle;
import com.google.android.gms.maps.model.CircleOptions;
import com.google.android.gms.maps.model.GroundOverlay;
import com.google.android.gms.maps.model.GroundOverlayOptions;
import com.google.android.gms.maps.model.LatLng;
import com.google.android.gms.maps.model.LatLngBounds;
import com.google.android.gms.maps.model.Marker;
import com.google.android.gms.maps.model.MarkerOptions;
import com.google.android.gms.maps.model.Polyline;
import com.google.android.gms.maps.model.PolylineOptions;
import com.google.gson.Gson;
import java.lang.reflect.Type;


import com.google.gson.reflect.TypeToken;
import com.indooratlas.android.sdk.IALocationManager;
import com.indooratlas.android.sdk.resources.IAFloorPlan;
import com.indooratlas.android.sdk.resources.IALatLng;
import com.indooratlas.android.sdk.resources.IAResourceManager;
import com.indooratlas.android.sdk.resources.IAResult;
import com.indooratlas.android.sdk.resources.IAResultCallback;
import com.indooratlas.android.sdk.resources.IATask;
import com.squareup.picasso.Picasso;
import com.squareup.picasso.RequestCreator;
import com.squareup.picasso.Target;

import org.json.JSONArray;
import org.json.JSONObject;

import java.util.ArrayList;
import java.util.List;

import no.hesa.positionlibrary.Point;
import no.hesa.positionlibrary.PositionLibrary;
import no.hesa.positionlibrary.api.ActionInterface;
import no.hesa.positionlibrary.api.Api;
import no.hesa.positionlibrary.dijkstra.exception.PathNotFoundException;
import no.hesa.positionlibrary.dijkstra.model.Edge;
import no.hesa.positionlibrary.dijkstra.model.Vertex;

@TargetApi(Build.VERSION_CODES.JELLY_BEAN_MR2)
public class MapActivity extends AppCompatActivity implements OnMapReadyCallback,ActionInterface{

    private static final String TAG = "MapActivity";
    private final LatLng UIT_NARVIK_POSITION = new LatLng(68.43590708f, 17.43452958f);
    private final float UIT_NARVIK_ZOOMLEVEL = 17.4f;
    private final int UIT_NARVIK_DEFAULTFLOOR = 1;
    private final int POLYLINEWIDTH = 20;
    private String DEFAULT_FLOORPLAN;

    public final static int SEARCH_RETURNED_COORDINATE_CODE = 1;
    public final static int SEARCH_RETURNED_COORDINATE_RESULT = 1;

    /* used to decide when bitmap should be downscaled */
    private static final int MAX_DIMENSION = 2048;

    private GoogleMap mMap; // Might be null if Google Play services APK is not available.
    private Marker mMarker;
    private GroundOverlay mGroundOverlay;
    private IALocationManager mIALocationManager;
    private IAResourceManager mResourceManager;
    private IATask<IAFloorPlan> mFetchFloorPlanTask;
    private Target mLoadTarget;
    private Intent returnedCoordsFromSearchIntent;
    private PositionLibrary positionLibrary = null;

    private BroadcastReceiver positionLibOutputReceiver = null;
    private BroadcastReceiver pathPositionLibReceiver = null;
    private BroadcastReceiver searchLocationReceiver = null;

    private LatLng currentPosition = null;
    private LatLng targetPosition = null;

    private Polyline polyline = null;

    private Menu menuRef = null;

    private LatLngBounds elevationChangeBounds = null;

    private String currentFloorPlan;
    private int currentFloor = -100;
    private ArrayList<ArrayList<Point>> receivedPath = null;

    private ProgressBar fetchMapSpinner;
    private boolean positioningEnabled = false;

    private long lastPress;

    private Circle circleThree;
    private Circle circleTwo;
    private Circle circleOne;

    ArrayList<Polyline> polylineList = new ArrayList<Polyline>();
    ArrayList<LatLng> latLngList = new ArrayList<LatLng>();
    ArrayList<Circle> circleList = new ArrayList<Circle>();

    private int mapType;
    private String pathPointJson;

    private SearchView searchView;
    private Api api;
    private boolean pathPointsDownloading = false;

    private List<Point> path;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        requestWindowFeature(Window.FEATURE_ACTION_BAR_OVERLAY);
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_map);

        DEFAULT_FLOORPLAN = getResources().getString(R.string.indooratlas_floor_2_floorplanid);

        // instantiate IALocationManager and IAResourceManager
        mIALocationManager = IALocationManager.create(this);
        mResourceManager = IAResourceManager.create(this);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        getSupportActionBar().setDisplayShowTitleEnabled(true);
        getSupportActionBar().setTitle("ARGH!");
        getSupportActionBar().setDisplayShowHomeEnabled(true);

        if(getResources().getConfiguration().locale.getISO3Country().compareTo("NOR") == 0) {
            showCustomToast(getApplicationContext(), "Norsk locale", Toast.LENGTH_SHORT);
            getSupportActionBar().setIcon(R.mipmap.ic_uit_logo_nor);
        }
        else {
            showCustomToast(getApplicationContext(), getResources().getConfiguration().locale.toString(), Toast.LENGTH_SHORT);
            getSupportActionBar().setIcon(R.mipmap.ic_uit_logo);
        }

        fetchMapSpinner = (ProgressBar)findViewById(R.id.fetch_map_progress_bar);
        positionLibrary = new PositionLibrary();
        registerButtonListeners();
/*
        //Api api = new Api(this, getApplicationContext().getResources());

        api.allUsers();
*/

        api = new Api(this);
        returnedCoordsFromSearchIntent = getIntent();
    }

    @Override
    protected void onPause() {
        super.onPause();

        if (mMap != null) {
            SharedPreferences.Editor sharedPreferences = getSharedPreferences("MapActivityPrefs", MODE_PRIVATE).edit();

            float zoomLevel = mMap.getCameraPosition().zoom;
            sharedPreferences.putFloat("ZoomLevel", zoomLevel);
            sharedPreferences.putInt("MapType", mapType);

            if (currentPosition != null) {
                float lat = (float) currentPosition.latitude;
                float lng = (float) currentPosition.longitude;
                sharedPreferences.putFloat("CurrentLat", lat);
                sharedPreferences.putFloat("CurrentLng", lng);
            }

            if (!currentFloorPlan.isEmpty()) {
                sharedPreferences.putString("CurrentFloorPlan", currentFloorPlan);
            }

            if (currentFloor != -100)
            {
                sharedPreferences.putInt("CurrentFloor", currentFloor);
            }

            if (path != null)
            {
                Gson gson = new Gson();
//                java.lang.reflect.Type listType = new TypeToken<List<Point>>(){}.getType();
                String json = gson.toJson(path /*, listType */);

                sharedPreferences.putString("PathJson", json);
            }
            sharedPreferences.apply();
        }
    }

    @Override
    protected void onResume() {
        super.onResume();
        // get map fragment reference
        SupportMapFragment mapFragment = (SupportMapFragment) getSupportFragmentManager().findFragmentById(R.id.map);
        mapFragment.getMapAsync(this);
    }

    @Override
    protected void onStart()
    {
        super.onStart();
        // TODO: check for sharedpreference
        SharedPreferences sharedPreferences = getSharedPreferences("MapActivityPrefs",MODE_PRIVATE);

        currentFloorPlan = sharedPreferences.getString("CurrentFloorPlan", DEFAULT_FLOORPLAN);
        fetchFloorPlan(currentFloorPlan);
    }

    @Override
    protected void onStop()
    {
        super.onStop();
        /*
        if (positionLibrary != null) {
            positionLibrary.wifiPosition.unRegisterBroadcast(this);
        }
*/
        if (positionLibOutputReceiver != null) {
            unregisterReceiver(positionLibOutputReceiver);
        }

        if (pathPositionLibReceiver != null) {
            unregisterReceiver(pathPositionLibReceiver);
        }

        if (searchLocationReceiver != null) {
            unregisterReceiver(searchLocationReceiver);
        }
    }

    @Override
    public void onMapReady(GoogleMap googleMap) {
        mMap = googleMap;

        SharedPreferences sharedPreferences = getSharedPreferences("MapActivityPrefs",MODE_PRIVATE);

        mapType = sharedPreferences.getInt("MapType", googleMap.MAP_TYPE_NONE);
        float zoomLevel = sharedPreferences.getFloat("ZoomLevel", 17.0f);
        float lat = sharedPreferences.getFloat("CurrentLat", (float)UIT_NARVIK_POSITION.latitude);
        float lng = sharedPreferences.getFloat("CurrentLng", (float)UIT_NARVIK_POSITION.longitude);
        currentFloor = sharedPreferences.getInt("CurrentFloor", UIT_NARVIK_DEFAULTFLOOR);
        currentPosition = new LatLng(lat, lng);

        positioningEnabled = sharedPreferences.getBoolean("PositioningEnabled", false);
        enablePositioning(positioningEnabled, false);

        mMap.setMapType(mapType);

        pathPointJson = sharedPreferences.getString("PathPointJson", null);
        /*
        String pathJson = sharedPreferences.getString("PathJson", null);
        Gson gson = new Gson();

        java.lang.reflect.Type listType = new TypeToken<List<Point>>(){}.getType();

        path = gson.toJson(pathJson, listType);
        */

        // TODO: 28/04/2016 first run only, maybe change to users position via GPS
        mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(lat, lng), zoomLevel));

        if (pathPointJson != null) {
//            showCustomToast(getApplicationContext(), "USING CACHED PATHPOINTS", Toast.LENGTH_SHORT);
            if (returnedCoordsFromSearchIntent != null) {
                if (returnedCoordsFromSearchIntent.getAction() != null) {
                    if (returnedCoordsFromSearchIntent.getAction().equals("no.hesa.veiviserennarvik.LAT_LNG_RETURN")) {
                        LatLng latLng = new LatLng(returnedCoordsFromSearchIntent.getDoubleExtra("lat", 0), returnedCoordsFromSearchIntent.getDoubleExtra("lng", 0));
                        double floor = returnedCoordsFromSearchIntent.getDoubleExtra("floor", 1.0);
//                        changeFloor((int)floor);
                        latLng = new LatLng(68.4358635893339, 17.434213757514954);
                        //floor = 1;

                        //currentPosition =  new LatLng(68.43548946533, 17.4339371547);
                        // currentFloor = 1;
                        if (floor != currentFloor) {
                            changeFloor(currentFloor);
                        }

                        try {
                            path = positionLibrary.wifiPosition.plotRoute(new Point(currentPosition.latitude, currentPosition.longitude, currentFloor), new Point(latLng.latitude, latLng.longitude, (int) floor), pathPointJson);
                            if (path != null) {
                                // draw path for this floor if available
                                List<Point> floorToDraw = null;
                                if (path != null) {
                                    floorToDraw = new ArrayList<Point>();
                                    for (Point point : path) {
                                        if (point.getFloor() == currentFloor) {
                                            floorToDraw.add(point);
                                        }
                                    }
                                }
                                if (floorToDraw != null) {
                                    drawFloorPath(floorToDraw);
                                    mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(path.get(0).getLatitude(), path.get(0).getLongitude()), 19));
                                }
                            }
                        }
                        catch (PathNotFoundException ex)
                        {
                            showCustomToast(getApplicationContext(), getResources().getString(R.string.path_not_found_exception), Toast.LENGTH_SHORT );
                        }

                        targetPosition = latLng;
                    }
                }
            }
        }
        else {
            /*
            Pathpoints are potentially a substantial download, and to avoid downloading them via
            the positioning-library they are retrieved and cached in this activity better suited for handling
            android lifecycle changes, then sent to the library when it's time to plot a route
             */

            // the searchView is not instantiated at this point,
            // so we set a variable to hide the search field after initialization
            pathPointsDownloading = true;
            api.allPathPoints();
        }

        registerOnMapClickReceiver();
//        registerPositionReceiver();
//        registerPathReceiver();
//        registerSearchLocationReceiver();
    }

    /**
     * Places markers (circles) with polylines connecting them along provided path.
     * Colors first and last circle differently.
     *
     * @param coordinateList
     * @return LatLang containing the last point added
     */
    private LatLng drawFloorPath(List<Point> coordinateList)
    {
        clearDrawnPaths();

        LatLng currentPoint = null;
        LatLng previousPoint = null;

        final double CIRCLERADIUS = 1;
        final double DOTRADIUS = 0.20;
        final double SMALLCIRCLERADIUS = 0.10;

        for(int i = 0; i < coordinateList.size(); i++)
        {
            currentPoint = new LatLng(coordinateList.get(i).getLatitude(), coordinateList.get(i).getLongitude());
            latLngList.add(currentPoint);

            // first point
            if (latLngList.size() == 1) {
                drawUserPosition(currentPoint);
            }

            if (latLngList.size() > 1) {
                previousPoint = new LatLng(coordinateList.get(i - 1).getLatitude(), coordinateList.get(i - 1).getLongitude());

                PolylineOptions po = new PolylineOptions()
                        .add(previousPoint, currentPoint)
                        .width(POLYLINEWIDTH)
                        .color(getResources().getColor(R.color.route_polyline_color))
                        .geodesic(false)
                        .zIndex(0.45f);

                polylineList.add(mMap.addPolyline(po));

                // all points except first and last
                if (latLngList.size() < coordinateList.size()) {
                    CircleOptions co = new CircleOptions()
                            .center(currentPoint)
                            .radius(SMALLCIRCLERADIUS)
                            .strokeColor(getResources().getColor(R.color.route_circle_color))
                            .fillColor(getResources().getColor(R.color.route_circle_color))
                            .zIndex(0.61f);

                    circleList.add(mMap.addCircle(co));
                }
                else { // last point
                    CircleOptions co = new CircleOptions()
                            .center(currentPoint)
                            .radius(CIRCLERADIUS)
                            .strokeColor(getResources().getColor(R.color.route_circle_end_border_color))
                            .fillColor(getResources().getColor(R.color.route_circle_end_color))
                            .zIndex(0.61f);

                    circleList.add(mMap.addCircle(co));

                    co = new CircleOptions()
                            .center(currentPoint)
                            .radius(DOTRADIUS)
                            .strokeColor(getResources().getColor(R.color.route_circle_end_border_color))
                            .fillColor(getResources().getColor(R.color.route_circle_end_border_color))
                            .zIndex(0.61f);

                    circleList.add(mMap.addCircle(co));
                }
            }
        }
        return currentPoint;
    }

    private void clearDrawnPaths()
    {
        for (Polyline polyline : polylineList) {
            polyline.remove();
        }

        for (Circle circle : circleList) {
            circle.remove();
        }

        polylineList.clear();
        latLngList.clear();
        circleList.clear();
    }

    private void drawUserPosition(LatLng currentPosition)
    {
        if (circleOne != null)
            circleOne.remove();

        CircleOptions co = new CircleOptions()
                .center(currentPosition)
                .radius(1)
                .strokeColor(getResources().getColor(R.color.route_circle_end_border_color))
                .fillColor(getResources().getColor(R.color.route_circle_end_color))
                .zIndex(0.50f);

        circleOne = mMap.addCircle(co);
    }

    private void changeFloor(int floor)
    {
        switch (floor) {
            case 0:
                break;
            case 1:
                fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_1_floorplanid));
                currentFloorPlan = getResources().getString(R.string.indooratlas_floor_1_floorplanid);
                currentFloor = 1;
                break;
            case 2:
                fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_2_floorplanid));
                currentFloorPlan = getResources().getString(R.string.indooratlas_floor_2_floorplanid);
                currentFloor = 2;
                break;
            case 3:
                break;
            case 4:
                break;
            case 5:
                break;
            default:
                break;
        }
    }
/*
    // http://stackoverflow.com/questions/15319431/how-to-convert-a-latlng-and-a-radius-to-a-latlngbounds-in-android-google-maps-ap
    // http://googlemaps.github.io/android-maps-utils/
    public LatLngBounds toBounds(LatLng center, double radius) {
        LatLng southwest = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 225);
        LatLng northeast = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 45);
        return new LatLngBounds(southwest, northeast);
    }
    */
//region BUTTON LISTENERS
    //// TODO: 28/04/2016 hardcoded floor values when changing floor with buttons 
    private void registerButtonListeners()
    {
        FloatingActionButton floorUpFab = (FloatingActionButton) findViewById(R.id.floor_up);
        floorUpFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {                
                if (currentFloorPlan.compareTo(getResources().getString(R.string.indooratlas_floor_1_floorplanid)) == 0) {
                    changeFloor(2);
                    mMap.clear();
                    // draw path for this floor if availableList
                    List<Point> floorToDraw = null;
                    if (path != null) {
                        floorToDraw = new ArrayList<Point>();
                        for (Point point : path) {
                            if (point.getFloor() == currentFloor) {
                                floorToDraw.add(point);
                            }
                        }
                    }
                    if (floorToDraw != null) {
                        drawFloorPath(floorToDraw);
                    }
                }
            }
        });

        FloatingActionButton floorDownFab = (FloatingActionButton) findViewById(R.id.floor_down);
        floorDownFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (currentFloorPlan.compareTo(getResources().getString(R.string.indooratlas_floor_2_floorplanid)) == 0) {
                    changeFloor(1);
                    mMap.clear();
                    // draw path for this floor if availableList
                    List<Point> floorToDraw = null;
                    if (path != null) {
                        floorToDraw = new ArrayList<Point>();
                        for (Point point : path) {
                            if (point.getFloor() == currentFloor) {
                                floorToDraw.add(point);
                            }
                        }
                    }
                    if (floorToDraw != null) {
                        drawFloorPath(floorToDraw);
                    }
                }
            }
        });

        FloatingActionButton automaticPositioningFab = (FloatingActionButton) findViewById(R.id.fab_automatic_positioning);
        automaticPositioningFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                enablePositioning(positioningEnabled, true);
            }
        });

        automaticPositioningFab.setOnLongClickListener(new View.OnLongClickListener() {
            @Override
            public boolean onLongClick(View v) {
                showCustomToast(getApplicationContext(), getResources().getString(R.string.refresh_pathpoints), Toast.LENGTH_LONG);
                // disable search, show loading etc
                searchView.setVisibility(View.INVISIBLE);
                api.allPathPoints();
                return true;
            }
        });

        FloatingActionButton myLocationFab = (FloatingActionButton) findViewById(R.id.fab_my_location);
        myLocationFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (currentPosition != null) {
                    mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(currentPosition, 20));
                }
            }
        });

        myLocationFab.setOnLongClickListener(new View.OnLongClickListener() {
            @Override
            public boolean onLongClick(View v) {
                currentPosition = UIT_NARVIK_POSITION;
                mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(currentPosition, UIT_NARVIK_ZOOMLEVEL));
                return true;
            }
        });

        FloatingActionButton switchMapTypeFab = (FloatingActionButton) findViewById(R.id.fab_switch_map_type);
        switchMapTypeFab.setOnClickListener(new View.OnClickListener(){
            @Override
            public void onClick(View view) {
                FloatingActionButton switchMapTypeFab = (FloatingActionButton) findViewById(R.id.fab_switch_map_type);

                if (mapType == mMap.MAP_TYPE_NORMAL) {
                    switchMapTypeFab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_location_city_white_24dp));
                    mapType = mMap.MAP_TYPE_NONE;
                }
                else {
                    switchMapTypeFab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_public_white_24dp));
                    mapType = mMap.MAP_TYPE_NORMAL;
                }
                mMap.setMapType(mapType);
            }
        });
    }
//endregion

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

    private void enablePositioning(boolean positioningEnabled, boolean showToast)
    {
        SharedPreferences.Editor sharedPreferences = getSharedPreferences("MapActivityPrefs", MODE_PRIVATE).edit();
        if (!positioningEnabled) // turn on
        {
            FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab_automatic_positioning);
            fab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_location_on_white_24dp));
            this.positioningEnabled = true;
            sharedPreferences.putBoolean("PositioningEnabled", positioningEnabled);

            if (showToast) {
                showCustomToast(getApplicationContext(), getResources().getString(R.string.positioning_automatic_on), Toast.LENGTH_SHORT);
            }
            registerPositionReceiver();
        }
        else // turn off
        {
            FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab_automatic_positioning);
            fab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_location_off_white_24dp));
            this.positioningEnabled = false;
            sharedPreferences.putBoolean("PositioningEnabled", positioningEnabled);

            if (positionLibOutputReceiver != null) {
                unregisterReceiver(positionLibOutputReceiver);
                positionLibOutputReceiver = null;
                if (showToast) {
                    showCustomToast(getApplicationContext(), getResources().getString(R.string.positioning_automatic_off), Toast.LENGTH_SHORT);
                }
            }
        }
        sharedPreferences.apply();
    }

//region BROADCASTRECEIVERS
    private void registerOnMapClickReceiver()
    {
        // adds a marker
        mMap.setOnMapClickListener(new GoogleMap.OnMapClickListener() {
            @Override
            public void onMapClick(LatLng point) {
                if (mMarker != null) {
                    mMarker.remove();
                }
                // new CircleOptions().center(point).radius(3.0).fillColor(R.color.radiusfillcolor).strokeColor(R.color.radiusstrokecolor).strokeWidth(2)
                mMarker = mMap.addMarker(new MarkerOptions().position(point).title("Lat: " + point.latitude + " Lng: " + point.longitude));
                mMarker.setDraggable(true);
                mMarker.showInfoWindow();
                currentPosition = new LatLng(point.latitude, point.longitude);
                clearDrawnPaths();
                drawUserPosition(currentPosition);
            }
        });
    }

    private void registerPositionReceiver()
    {
        positionLibOutputReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                if (intent.getAction().equals("no.hesa.positionlibrary.Output")) {
                    double[] pos = intent.getDoubleArrayExtra("position");
                    int floor = intent.getIntExtra("floor", 1);
                    if (pos[0] != 0 && pos[1] != 0)
                    {
                        LatLng latLng = new LatLng(pos[0], pos[1]);

                        currentPosition = latLng;

                        drawUserPosition(currentPosition);
                        //TODO: set to current zoom level, but have the first update zoom to 19
                        mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(currentPosition, 19));

                        if (floor != currentFloor) {
                            changeFloor(floor);
                            mMap.clear();
                        }

                        currentFloor = floor;
/*
                        if (elevationChangeBounds != null) {
                            if (elevationChangeBounds.contains(latLng)) {
                                drawNextFloor(); // loads the next map and draws the path when end point is reached
                            }
                        }
*/
                    }
                    else
                    {
                        showCustomToast(MapActivity.this, getResources().getString(R.string.positioning_unable_to_locate_user), Toast.LENGTH_LONG);
                    }
                }
            }
        };
        positionLibrary.wifiPosition.registerBroadcast(this);
        registerReceiver(positionLibOutputReceiver, new IntentFilter("no.hesa.positionlibrary.Output"));
    }

    private void registerPathReceiver()
    {

    }

/*
    private void registerSearchLocationReceiver()
    {
     //   searchLocationReceiver = new BroadcastReceiver() {
        class SearchLocationReceiver extends BroadcastReceiver {
        @Override
        public void onReceive(Context context, Intent intent) {
            if (intent.getAction().equals("no.hesa.veiviserennarvik.LAT_LNG_RETURN")) {
                Toast.makeText(MapActivity.this, "intentReceiver onReceive method", Toast.LENGTH_LONG).show();
                LatLng latLng = new LatLng(intent.getDoubleExtra("lat",0),intent.getDoubleExtra("lng",0));
                mMap.addMarker(new MarkerOptions().position(latLng).title("TestLocFromBR"));
                mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(latLng, 17));
                mMap.addCircle(new CircleOptions().center(latLng).radius(3.0).fillColor(R.color.radiusfillcolor).strokeColor(R.color.radiusstrokecolor).strokeWidth(2));
                targetPosition = latLng;
                }
            }
        };
        searchLocationReceiver = new SearchLocationReceiver();
        registerReceiver(searchLocationReceiver, new IntentFilter("no.hesa.veiviserennarvik.LAT_LNG_RETURN"));
    }
    */
//endregion

//region INDOORATLAS METHODS
        /**
         * Sets bitmap of floor plan as ground overlay on Google Maps
         */
    private void setupGroundOverlay(IAFloorPlan floorPlan, Bitmap bitmap) {
        if (mGroundOverlay != null) {
            mGroundOverlay.remove();
        }

        if (mMap != null) {
            BitmapDescriptor bitmapDescriptor = BitmapDescriptorFactory.fromBitmap(bitmap);
            IALatLng iaLatLng = floorPlan.getCenter();
            LatLng center = new LatLng(iaLatLng.latitude, iaLatLng.longitude);
            GroundOverlayOptions fpOverlay = new GroundOverlayOptions()
                    .image(bitmapDescriptor)
                    .position(center, floorPlan.getWidthMeters(), floorPlan.getHeightMeters())
                    .bearing(floorPlan.getBearing())
                    .zIndex(0.1f);

            mGroundOverlay = mMap.addGroundOverlay(fpOverlay);
            fetchMapSpinner.setVisibility(View.GONE); // hide loading spinner, load complete
        }
    }

    /**
     * Download floor plan using Picasso library.
     */
    private void fetchFloorPlanBitmap(final IAFloorPlan floorPlan) {

        final String url = floorPlan.getUrl();

        if (mLoadTarget == null) {
            mLoadTarget = new Target()
            {
                @Override
                public void onBitmapLoaded(Bitmap bitmap, Picasso.LoadedFrom from)
                {
                    Log.d(TAG, "onBitmap loaded with dimensions: " + bitmap.getWidth() + "x"
                            + bitmap.getHeight());
                    setupGroundOverlay(floorPlan, bitmap);
                }

                @Override
                public void onPrepareLoad(Drawable placeHolderDrawable)
                {
                    // N/A
                }

                @Override
                public void onBitmapFailed(Drawable placeHolderDraweble)
                {
                    showCustomToast(getApplicationContext(), getResources().getString(R.string.failed_to_load_map), Toast.LENGTH_SHORT);
                    fetchMapSpinner.setVisibility(View.GONE);
                }
            };
        }

        RequestCreator request = Picasso.with(this).load(url);

        final int bitmapWidth = floorPlan.getBitmapWidth();
        final int bitmapHeight = floorPlan.getBitmapHeight();

        if (bitmapHeight > MAX_DIMENSION) {
            request.resize(0, MAX_DIMENSION);
        }
        else if (bitmapWidth > MAX_DIMENSION) {
            request.resize(MAX_DIMENSION, 0);
        }
        request.into(mLoadTarget);
    }

    /**
     * Fetches floor plan data from IndoorAtlas server.
     */
    private void fetchFloorPlan(String id) {
        fetchMapSpinner.setVisibility(View.VISIBLE); // show loading spinner

        // if there is already running task, cancel it
        cancelPendingNetworkCalls();

        final IATask<IAFloorPlan> task = mResourceManager.fetchFloorPlanWithId(id);

        task.setCallback(new IAResultCallback<IAFloorPlan>()
        {
            @Override
            public void onResult(IAResult<IAFloorPlan> result)
            {
                if (result.isSuccess() && result.getResult() != null) {
                    // retrieve bitmap for this floor plan metadata
                    fetchFloorPlanBitmap(result.getResult());
                }
                else {
                    // ignore errors if this task was already canceled
                    if (!task.isCancelled()) {
                        // do something with error
                        showCustomToast(MapActivity.this, getResources().getString(R.string.loading_floorplan_failed) + ": " + result.getError(), Toast.LENGTH_SHORT);
                        // remove current ground overlay
                        if (mGroundOverlay != null) {
                            mGroundOverlay.remove();
                            mGroundOverlay = null;
                        }
                    }
                }
            }
        }, Looper.getMainLooper()); // deliver callbacks using main looper
        // keep reference to task so that it can be canceled if needed
        mFetchFloorPlanTask = task;
    }

    /**
     * Helper method to cancel current task if any.
     */
    private void cancelPendingNetworkCalls() {
        if (mFetchFloorPlanTask != null && !mFetchFloorPlanTask.isCancelled()) {
            mFetchFloorPlanTask.cancel();
        }
    }

//endregion

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        menuRef = menu;
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.menu_map, menu);
        getSupportActionBar().setDisplayShowTitleEnabled(false); // hides toolbar title

        SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);
        String token = sharedPreferences.getString("Code",Api.NO_TOKEN);
        if (token.length() > 0) {
            menu.findItem(R.id.action_login).setVisible(false); // logIN menu item
            menu.findItem(R.id.action_logout).setVisible(true); // logOUT menu item
        } else {
            menu.findItem(R.id.action_login).setVisible(true); // logIN menu item
            menu.findItem(R.id.action_logout).setVisible(false); // logOUT menu item
        }

        // Retrieve the SearchView and plug it into SearchManager
        searchView = (SearchView) MenuItemCompat.getActionView(menu.findItem(R.id.action_search));
        // searchView.setIconifiedByDefault(false); // autoexpands the searchfield
        if (pathPointsDownloading) {
            searchView.setVisibility(View.INVISIBLE);
        }
        searchView.setMaxWidth(Integer.MAX_VALUE); // sets the width of the search field to a very large number to keep it maximized
        SearchManager searchManager = (SearchManager) getSystemService(Context.SEARCH_SERVICE);
        searchView.setSearchableInfo(searchManager.getSearchableInfo(new ComponentName("no.hesa.veiviserenuitnarvik","no.hesa.veiviserenuitnarvik.SearchResultsActivity")));

        // listener for toolbar search submit
        searchView.setOnQueryTextListener(new SearchView.OnQueryTextListener() {

            @Override
            public boolean onQueryTextSubmit(String newText) {
                Intent intent = new Intent(getApplicationContext(), SearchResultsActivity.class);
                intent.setAction(Intent.ACTION_SEARCH);
                intent.putExtra("query", newText);
                searchView.setQuery("", false); // clears the searchview without submitting
                searchView.clearFocus();
                startActivityForResult(intent, SEARCH_RETURNED_COORDINATE_CODE );
                return true;
            }

            @Override
            public boolean onQueryTextChange(String newText) {
                return false;
            }

        });
        return true;

    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        // Handle action bar item clicks here. The action bar will
        // automatically handle clicks on the Home/Up button, so long
        // as you specify a parent activity in AndroidManifest.xml.
        switch (item.getItemId()) {
            case R.id.action_login:
                Intent intent = new Intent(this,AuthenticationActivity.class);
                startActivity(intent);
                break;
            case R.id.action_logout:
                SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);
                SharedPreferences.Editor editor = sharedPreferences.edit();
                editor.remove("Code");
                editor.apply();
                menuRef.findItem(R.id.action_login).setVisible(true); // logIN menu item
                menuRef.findItem(R.id.action_logout).setVisible(false); // logOUT menu item
                break;
            default:
                break;
        }
        return super.onOptionsItemSelected(item);
    }

    @Override
    public void onCompletedAction(JSONObject jsonObject, String actionString) {

        switch (actionString) {
            case Api.ALL_USERS:
                //JSONObject dummyObject = jsonObject;
                break;
            case Api.ALL_PATH_POINTS:
                try {
                    if(jsonObject != null){
                        // enable search, show download completed etc
                        searchView.setVisibility(View.VISIBLE);
                        pathPointsDownloading = false;
                        pathPointJson = jsonObject.toString();

                        /*
                         SharedPreferences can theoretically hold a string 2^31 bits in length,
                         however you are limited to heap size which can be as low as 16MB on some devices.
                         The 141 path points in our test enviroment are 103kB so for simplicitys sake (and time constraints)
                         we will cache the pathpoint list in sharedpreferences. Ideally we would set up an SQLite DB
                         combined with an API call returning a hash (to determine if the pathlist has been updated)
                         for scaling with a very large amount of points.
                         */
                        SharedPreferences.Editor sharedPreferences = getSharedPreferences("MapActivityPrefs", MODE_PRIVATE).edit();
                        sharedPreferences.putString("PathPointJson", jsonObject.toString());
                        sharedPreferences.apply();
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
                break;

            default:
                break;
        }
    }

    @Override
    public void onAuthorizationFailed() {
        //TODO: possible intent loop?
        SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);

        if (sharedPreferences.getBoolean("LoggedInThisSession", true)) {
            Intent startAuthorization = new Intent(this, AuthenticationActivity.class);
            startActivity(startAuthorization);
        }
    }

    // http://stackoverflow.com/questions/14006461/android-confirm-app-exit-with-toast/18654014#18654014
    @Override
    public void onBackPressed() {
        long currentTime = System.currentTimeMillis();
        if(currentTime - lastPress > 5000){
            showCustomToast(getApplicationContext(), getResources().getString(R.string.close_mapactivity_backpress_confirmation), Toast.LENGTH_SHORT);
            lastPress = currentTime;
        }
        else{
            super.onBackPressed();
        }
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == SEARCH_RETURNED_COORDINATE_CODE) {
            if(resultCode == SEARCH_RETURNED_COORDINATE_RESULT){
                LatLng latLng = new LatLng(data.getDoubleExtra("lat", 0), data.getDoubleExtra("lng", 0));
                double floor = data.getDoubleExtra("floor", 1.0);


                showCustomToast(getApplicationContext(), latLng.toString() + ", " + floor, Toast.LENGTH_SHORT );

//              changeFloor((int)floor);
                latLng = new LatLng(68.4358635893339, 17.434213757514954);
                floor = 1;

//                currentPosition =  new LatLng(68.43548946533, 17.4339371547);
//                currentFloor = 1;
                if (floor != currentFloor) {
                    changeFloor(currentFloor);
                }

                try {
                    path = positionLibrary.wifiPosition.plotRoute(new Point(currentPosition.latitude, currentPosition.longitude, currentFloor), new Point(latLng.latitude, latLng.longitude, (int) floor), pathPointJson);
                    if (path != null) {
                        // draw path for this floor if available
                        List<Point> floorToDraw = null;
                        if (path != null) {
                            floorToDraw = new ArrayList<Point>();
                            for (Point point : path) {
                                if (point.getFloor() == currentFloor) {
                                    floorToDraw.add(point);
                                }
                            }
                        }
                        if (floorToDraw != null) {
                            drawFloorPath(floorToDraw);
                            mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(path.get(0).getLatitude(), path.get(0).getLongitude()), 19));
                        }
                    }
                }
                catch (PathNotFoundException ex)
                {
                    showCustomToast(getApplicationContext(), getResources().getString(R.string.path_not_found_exception), Toast.LENGTH_SHORT );
                }

                targetPosition = latLng;
            }
        }
    }
}
