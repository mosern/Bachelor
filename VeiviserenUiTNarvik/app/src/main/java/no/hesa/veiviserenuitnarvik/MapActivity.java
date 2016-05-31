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


import com.google.android.gms.common.api.BooleanResult;
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


import com.google.gson.reflect.TypeToken;
import com.google.maps.android.SphericalUtil;
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

import org.json.JSONObject;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;

import no.hesa.positionlibrary.Point;
import no.hesa.positionlibrary.PositionLibrary;
import no.hesa.positionlibrary.api.ActionInterface;
import no.hesa.positionlibrary.api.Api;
import no.hesa.positionlibrary.dijkstra.exception.PathNotFoundException;

@TargetApi(Build.VERSION_CODES.JELLY_BEAN_MR2)
public class MapActivity extends AppCompatActivity implements OnMapReadyCallback, ActionInterface{

    // constants
    private static final String TAG = "MapActivity";
    private final LatLng UIT_NARVIK_POSITION = new LatLng(68.43590708f, 17.43452958f); // position directly in the center of UiT Campus Narvik
    private final float UIT_NARVIK_ZOOMLEVEL = 17.4f; // default zoom level
    private final int UIT_NARVIK_DEFAULTFLOOR = 1; // default floor
    private final int POLYLINEWIDTH = 20; // width of route generated
    public final static int SEARCH_RETURNED_COORDINATE_CODE = 1; // startActivityForResult request code to search activity
    public final static int SEARCH_RETURNED_COORDINATE_RESULT = 1; // startActivityForResult returned result code from search activity
    private String DEFAULT_FLOORPLAN; // default floorplan, set from resource in onCreate()

    // IndoorAtlas variables and class instances
    private static final int MAX_DIMENSION = 2048; // used to decide when bitmap should be downscaled
    private GroundOverlay mGroundOverlay;
    private IALocationManager mIALocationManager;
    private IAResourceManager mResourceManager;
    private IATask<IAFloorPlan> mFetchFloorPlanTask;
    private Target mLoadTarget;

    // google maps elements
    private GoogleMap mMap; // Might be null if Google Play services APK is not available.
    private Marker mMarker; //todo: remove after removing marker from onMapClick
    private int mapType; // type of google map to display

    // lists containing drawn elements, used for removing them on demand
    ArrayList<Polyline> polylineList = new ArrayList<Polyline>();
    ArrayList<LatLng> latLngList = new ArrayList<LatLng>();
    ArrayList<Circle> circleList = new ArrayList<Circle>();

    // class instances
    private PositionLibrary positionLibrary = null;
    private BroadcastReceiver positionLibOutputReceiver = null;
    private Api api;

    // global variables (necessary due to lifecycle changes, used in many methods, etc)
    private Menu menuRef = null;
    private String currentFloorPlan;

    private LatLng currentPosition = null;
    private int currentFloor = -100; // unlikely number

    private LatLng targetPosition = null;
    private int targetFloor = -101; // unlikely number

    private ProgressBar fetchMapSpinner; // loading spinner
    private boolean positioningEnabled = false; // indicator of automatic positioning is enabled or not

    private long backPressedTimeStamp;
    private Circle userPositionMarker;

    private List<Point> path;
    private ArrayList<List<Point>> fullSegmentedPath;

    private boolean pathPointsDownloading = false; // indicator if pathpoints are being downloaded or not

    private SearchView searchView;

    private String pathPointJson; // list of all pathpoints retrieved from API

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        requestWindowFeature(Window.FEATURE_ACTION_BAR_OVERLAY);
        super.onCreate(savedInstanceState);

        setContentView(R.layout.activity_map);

        DEFAULT_FLOORPLAN = getResources().getString(R.string.indooratlas_floor_1_floorplanid);

        // instantiate IALocationManager and IAResourceManager (IndoorAtlas)
        mIALocationManager = IALocationManager.create(this);
        mResourceManager = IAResourceManager.create(this);

        Toolbar toolbar = (Toolbar) findViewById(R.id.toolbar);
        setSupportActionBar(toolbar); // TODO: crashes pre android 5.0

        getSupportActionBar().setDisplayShowTitleEnabled(true);
        getSupportActionBar().setTitle("ARGH!");
        getSupportActionBar().setDisplayShowHomeEnabled(true);

        // changes the UiT logo based on system locale
        if(getResources().getConfiguration().locale.getISO3Country().compareTo("NOR") == 0) {
//            showCustomToast(getApplicationContext(), "Norsk locale", Toast.LENGTH_SHORT);
            getSupportActionBar().setIcon(R.mipmap.ic_uit_logo_nor);
        }
        else {
//            showCustomToast(getApplicationContext(), getResources().getConfiguration().locale.toString(), Toast.LENGTH_SHORT);
            getSupportActionBar().setIcon(R.mipmap.ic_uit_logo);
        }

        fetchMapSpinner = (ProgressBar)findViewById(R.id.fetch_map_progress_bar);
        positionLibrary = new PositionLibrary();
        api = new Api(this);
    }

    @Override
    protected void onPause() {
        super.onPause();
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

            sharedPreferences.putInt("CurrentFloor", currentFloor);

//            if (fullSegmentedPath != null) {
//                Gson gson = new Gson();
//                String json = gson.toJson(fullSegmentedPath);
//                sharedPreferences.putString("FullSegmentedPathJson", json);
//            }

            sharedPreferences.apply();
        }
        /*
        if (positionLibrary != null) {
            positionLibrary.wifiPosition.unRegisterBroadcast(this);
        }
*/
        if (positionLibOutputReceiver != null) {
            unregisterReceiver(positionLibOutputReceiver);
        }
    }

    /**
     * Is run after the googlemap is ready, meaning mMap variable is ready to use. Treat as onCreate for functions that require mMap
     * @param googleMap
     */
    @Override
    public void onMapReady(GoogleMap googleMap) {
        mMap = googleMap;

        // retrieve various sharedPreferences
        SharedPreferences sharedPreferences = getSharedPreferences("MapActivityPrefs",MODE_PRIVATE);

        mapType = sharedPreferences.getInt("MapType", googleMap.MAP_TYPE_NONE);
        mMap.setMapType(mapType);

        // gets saved value from sharedpreferences, otherwise sets default values
        float zoomLevel = sharedPreferences.getFloat("ZoomLevel", UIT_NARVIK_ZOOMLEVEL);
        float lat = sharedPreferences.getFloat("CurrentLat", (float)UIT_NARVIK_POSITION.latitude);
        float lng = sharedPreferences.getFloat("CurrentLng", (float)UIT_NARVIK_POSITION.longitude);
        currentFloor = sharedPreferences.getInt("CurrentFloor", UIT_NARVIK_DEFAULTFLOOR);
        currentPosition = new LatLng(lat, lng);

        pathPointJson = sharedPreferences.getString("PathPointJson", null);

        positioningEnabled = sharedPreferences.getBoolean("PositioningEnabled", false);
        enablePositioning(positioningEnabled, false); // turns automatic positioning on/off

        Gson gson = new Gson();
//        String pathJson = sharedPreferences.getString("PathJson", null);
//        java.lang.reflect.Type pathListType = new TypeToken<List<Point>>(){}.getType();
//        path = gson.fromJson(pathJson, pathListType);

//        String fullSegmentedPathJson = sharedPreferences.getString("FullSegmentedPathJson", null);
//        java.lang.reflect.Type fullSegmentedListType = new TypeToken<List<List<Point>>>(){}.getType();
//        fullSegmentedPath = gson.fromJson(fullSegmentedPathJson, fullSegmentedListType);

        mMap.moveCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(lat, lng), zoomLevel));

        if (pathPointJson == null) {
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
        registerButtonListeners();

//        if (fullSegmentedPath != null) {
//            drawFloorPath(fullSegmentedPath);
//        }
    }

//region DRAW/CLEAR METHODS
    /**
     * Places markers (circles) with polylines connecting them along provided path.
     * Indicates first and last circle differently.
     *
     * @param coordinateList
     * @return LatLang containing the last point added
     */
    private LatLng drawFloorPath(List<List<Point>> coordinateList)
    {
        clearDrawnPaths();

        LatLng currentPoint = null;
        LatLng previousPoint = null;

        final double CIRCLERADIUS = 1;
        final double DOTRADIUS = 0.20;
        final double SMALLCIRCLERADIUS = 0.10;

        for(List<Point> pointList : coordinateList){
            for(int i = 0; i < pointList.size(); i++)
            {
                if (pointList.get(i).getFloor() == currentFloor) {
                    currentPoint = new LatLng(pointList.get(i).getLatitude(), pointList.get(i).getLongitude());
                    latLngList.add(currentPoint);

                    // first point
                    if (latLngList.size() == 1) {
//                        drawUserPosition(currentPoint);
                        CircleOptions co = new CircleOptions()
                                .center(currentPoint)
                                .radius(CIRCLERADIUS)
                                .strokeColor(getResources().getColor(R.color.route_circle_end_border_color))
                                .fillColor(getResources().getColor(R.color.route_circle_end_color))
                                .zIndex(0.61f);

                        circleList.add(mMap.addCircle(co));
                    }

                    if (latLngList.size() > 1) {
                        previousPoint = new LatLng(pointList.get(i - 1).getLatitude(), pointList.get(i - 1).getLongitude());

                        PolylineOptions po = new PolylineOptions()
                                .add(previousPoint, currentPoint)
                                .width(POLYLINEWIDTH)
                                .color(getResources().getColor(R.color.route_polyline_color))
                                .geodesic(false)
                                .zIndex(0.45f);

                        polylineList.add(mMap.addPolyline(po));

                        // all points except first and last
                        if (latLngList.size() < pointList.size()) {
                            CircleOptions co = new CircleOptions()
                                    .center(currentPoint)
                                    .radius(SMALLCIRCLERADIUS)
                                    .strokeColor(getResources().getColor(R.color.route_circle_color))
                                    .fillColor(getResources().getColor(R.color.route_circle_color))
                                    .zIndex(0.61f);

                            circleList.add(mMap.addCircle(co));
                        } else { // last point
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
            }
            latLngList.clear();
        }
        return currentPoint;
    }

    /**
     * Clears all markings related to pathing
     */
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

    /**
     * Draws the current position, update currentPosition before running
     */
    private void drawUserPosition()
    {
        clearDrawnUserPosition();

        CircleOptions co = new CircleOptions()
                .center(currentPosition)
                .radius(1)
                .strokeColor(getResources().getColor(R.color.route_circle_end_border_color))
                .fillColor(getResources().getColor(R.color.route_circle_end_color))
                .zIndex(0.50f);

        //userPositionMarker = mMap.addCircle(co);
    }

    /**
     * Clears all marking related to the current position
     */
    private void clearDrawnUserPosition()
    {
        if (userPositionMarker != null) {
            userPositionMarker.remove();
        }
    }
//endregion

    /**
     * Changes floor, meaning it loads the floorplan for the indicated floor
     * @param floor
     */
    private void changeFloor(int floor)
    {
        switch (floor) {
            case 0:
                break;
            case 1:
                currentFloorPlan = getResources().getString(R.string.indooratlas_floor_1_floorplanid);
                currentFloor = 1;
                fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_1_floorplanid));
                break;
            case 2:
                currentFloorPlan = getResources().getString(R.string.indooratlas_floor_2_floorplanid);
                currentFloor = 2;
                fetchFloorPlan(getResources().getString(R.string.indooratlas_floor_2_floorplanid));
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

//region BUTTON LISTENERS
    // TODO: 28/04/2016 hardcoded floor values when changing floor with buttons

    /**
     * Registers all buttonClickListeners
     */
    private void registerButtonListeners()
    {
        // floor up button
        FloatingActionButton floorUpFab = (FloatingActionButton) findViewById(R.id.floor_up);
        floorUpFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {                
                if (currentFloorPlan.compareTo(getResources().getString(R.string.indooratlas_floor_1_floorplanid)) == 0) {
                    changeFloor(2);
                    clearDrawnPaths();
                    clearDrawnUserPosition();
                    // draw path for this floor if available
                    if (fullSegmentedPath != null) {
                        drawFloorPath(fullSegmentedPath);
                    }
                }
            }
        });

        // floor down button
        FloatingActionButton floorDownFab = (FloatingActionButton) findViewById(R.id.floor_down);
        floorDownFab.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                if (currentFloorPlan.compareTo(getResources().getString(R.string.indooratlas_floor_2_floorplanid)) == 0) {
                    changeFloor(1);
                    clearDrawnPaths();
                    clearDrawnUserPosition();
                    // draw path for this floor if available
                    if (fullSegmentedPath != null) {
                        drawFloorPath(fullSegmentedPath);
                    }
                }
            }
        });

        // toggle automatic positioning button, longclick to refresh pathpoints from API
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

        // animate camera to users position button, longclick to animate camera to a view of  the entire building
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
                mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(UIT_NARVIK_POSITION, UIT_NARVIK_ZOOMLEVEL));
                return true;
            }
        });

        // switch google map type button, switches between MAP_TYPE_NORMAL and MAP_TYPE_NONE
        FloatingActionButton switchMapTypeFab = (FloatingActionButton) findViewById(R.id.fab_switch_map_type);
        switchMapTypeFab.setOnClickListener(new View.OnClickListener(){
            @Override
            public void onClick(View view) {
                FloatingActionButton switchMapTypeFab = (FloatingActionButton) findViewById(R.id.fab_switch_map_type);

                if (mapType == mMap.MAP_TYPE_NORMAL) {
                    // switches the icon to indicate which map is shown
                    switchMapTypeFab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_location_city_white_24dp));
                    mapType = mMap.MAP_TYPE_NONE;
                }
                else {
                    // switches the icon to indicate which map is shown
                    switchMapTypeFab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_public_white_24dp));
                    mapType = mMap.MAP_TYPE_NORMAL;
                }
                mMap.setMapType(mapType);
            }
        });
    }
//endregion

    /**
     * Shows a custom toast, using toast.xml
     * @param context Context
     * @param msg The toast message
     * @param length Duration, use Toast.LENGTH_SHORT or LENGTH_LONG
     */
    private void showCustomToast(Context context, String msg, int length)
    {
        // inflates the layout containing the elements of the custom toast, so we can access them
        LayoutInflater inflater = getLayoutInflater();
        View toastLayout = inflater.inflate(R.layout.toast, (ViewGroup) findViewById(R.id.toast_layout));

        TextView text = (TextView) toastLayout.findViewById(R.id.toast_content); // reference to the now exposed textview
        text.setText(msg); // this is where we now set the toast message

        Toast toast = new Toast(getApplicationContext());
        toast.setView(toastLayout); // sets the toast view to our custom view
        toast.setDuration(length);
        toast.show();
    }

    /**
     * Switches automatic positioning via positionlibrary on/off
     * @param positioningEnabled True/false to turn positioning on/off
     * @param showToast True/false to show/not show toast indicating the current state
     */
    private void enablePositioning(boolean positioningEnabled, boolean showToast)
    {
        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab_automatic_positioning);
        SharedPreferences.Editor sharedPreferences = getSharedPreferences("MapActivityPrefs", MODE_PRIVATE).edit();
        if (!positioningEnabled) // turn on
        {
            // switches the icon to indicate if positioning is on or off
            fab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_location_on_white_24dp));
            this.positioningEnabled = true;
            sharedPreferences.putBoolean("PositioningEnabled", positioningEnabled);

            if (showToast) {
                showCustomToast(getApplicationContext(), getResources().getString(R.string.positioning_automatic_on), Toast.LENGTH_SHORT);
            }
            registerPositionReceiver(); // register the receiver to listen for location updates
        }
        else // turn off
        {
            // switches the icon to indicate if positioning is on or off
            fab.setImageDrawable(ContextCompat.getDrawable(getApplicationContext(), R.drawable.ic_location_off_white_24dp));
            this.positioningEnabled = false;
            sharedPreferences.putBoolean("PositioningEnabled", positioningEnabled);

            if (positionLibOutputReceiver != null) {
                unregisterReceiver(positionLibOutputReceiver); // unregisters the receiver that listens for location updates
                positionLibOutputReceiver = null;
                if (showToast) {
                    showCustomToast(getApplicationContext(), getResources().getString(R.string.positioning_automatic_off), Toast.LENGTH_SHORT);
                }
            }
        }
        sharedPreferences.apply();
    }

    /**
     * Registers the onMapClickreceiver, which is used for setting the current position of the user
     * to the clicked point on the map and clearing the old position
     */
    private void registerOnMapClickReceiver()
    {
        // sets the current position of the user to the clicked point on the map, clearing the old position
        mMap.setOnMapClickListener(new GoogleMap.OnMapClickListener() {
            @Override
            public void onMapClick(LatLng point) {
                // TODO: remove marker for launch)
                if (mMarker != null) {
                    mMarker.remove();
                }
                mMarker = mMap.addMarker(new MarkerOptions().position(point).title("Lat: " + point.latitude + " Lng: " + point.longitude));
                mMarker.setDraggable(true);
                mMarker.showInfoWindow();

                currentPosition = new LatLng(point.latitude, point.longitude);
                //todo: set currentFloor
                clearDrawnPaths();
                drawUserPosition();
            }
        });
    }

    /**
     * Registers the BroadcastReceiver which listens for position updates from positionlibrary.
     * It then, on receiving a calcuated position based on WiFi-trilateration, draws a route
     * to the target position if one has been selected. This works over different floors.
     */
    private void registerPositionReceiver()
    {
        positionLibOutputReceiver = new BroadcastReceiver() {
            @Override
            public void onReceive(Context context, Intent intent) {
                if (intent.getAction().equals("no.hesa.positionlibrary.Output")) {
                    double[] pos = intent.getDoubleArrayExtra("position");
                    int floor = intent.getIntExtra("floor", 1);
                    if (pos[0] != 0 && pos[1] != 0) // if a valid position is received (not 0,0)
                    {
                        LatLng latLng = new LatLng(pos[0], pos[1]);

                        // if the new position is on different floor to the current position, change the floor
                        if (floor != currentFloor) {
                            changeFloor(floor);
                            clearDrawnPaths();
                        }

                        clearDrawnUserPosition(); // clear markers for the last position

                        // set current position to the new one
                        currentPosition = latLng;
                        currentFloor = floor;

                        // if a target location exists, generate a path to it
                        if (targetPosition != null && targetFloor > -100) { // if target position has been set by ex. searching
                            if (path != null) {
                                // requests the positioning library calculate a path between the users position and the target
                                path = requestPathFromPosLib(targetPosition, targetFloor);
                                if (path.size() != 0) {
                                    // if a path is received, segments the received path (that can span different floors) into parts
                                    // ex. a path that goes from the 1st floor, up to the second floor, and then down to the 1st floor again would generate a 3 part path
                                    fullSegmentedPath = generateFullSegmentedPath(path);
                                } else {
                                    showCustomToast(getApplicationContext(), getResources().getString(R.string.path_not_found_exception), Toast.LENGTH_SHORT);
                                }
                            }
                        }

                        // if a segmented path exists, either from sharedpreferences in case of lifecycle changes or just fetched from positionlibrary
                        // send the segmented path to be drawn (only the parts on the visible floor will be drawn).
                        // otherwise just draw the user position
                        if (fullSegmentedPath != null) {
                            drawFloorPath(fullSegmentedPath);
                        }
                        else {
                            drawUserPosition();
                        }

                        // checks if the current point is within a specified radius of the last point of the path
                        // via a LatLngBounds, and if it is, determines you have arrived at your destination
                        if (path != null) {
                            if (path.size() != 0) {
                                Point currentPoint = new Point(latLng.latitude, latLng.longitude, floor);
                                if (arrivedAtFinalLocation(currentPoint, path.get(path.size() - 1))) //gets final point
                                {
                                    // unbind targetPosition etc etc
                                }
                            }
                        }

                        //TODO: set to current zoom level, but have the first update zoom to 19
                        mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(currentPosition, 19)); // animates the camera to the received position
                    }
                    else {
                        showCustomToast(MapActivity.this, getResources().getString(R.string.positioning_unable_to_locate_user), Toast.LENGTH_LONG);
                    }
                }
            }
        };
        positionLibrary.wifiPosition.registerBroadcast(this);
        registerReceiver(positionLibOutputReceiver, new IntentFilter("no.hesa.positionlibrary.Output"));
    }

    /**
     * Creates a LatLngBounds with a specified radius around finalLocation, then checks if the
     * LatLngBounds contains userpoint. This indicates that the user has arrived at his destination
     * @param userPoint Point to compared to
     * @param finalLocation Final location
     * @return True if userPoint is within the LatLngBounds created around finalLocation
     */
    public boolean arrivedAtFinalLocation(Point userPoint, Point finalLocation)
    {
        double radius = 4; // in meters
        LatLngBounds llb = toBounds(new LatLng((float)finalLocation.getLatitude(), (float)finalLocation.getLongitude()), radius);
        if (userPoint.getFloor() == finalLocation.getFloor()) {
            if (llb.contains(new LatLng((float) userPoint.getLatitude(), (float) userPoint.getLongitude()))) {
                return true;
            }
        }
        return false;
    }

    // http://stackoverflow.com/questions/15319431/how-to-convert-a-latlng-and-a-radius-to-a-latlngbounds-in-android-google-maps-ap
    // http://googlemaps.github.io/android-maps-utils/

    /**
     * Converts a LatLng and a radius to a LatLngBounds centered on the LatLng
     *
     * Method from http://stackoverflow.com/questions/15319431/how-to-convert-a-latlng-and-a-radius-to-a-latlngbounds-in-android-google-maps-ap
     * Uses http://googlemaps.github.io/android-maps-utils/
     * @param center
     * @param radius
     * @return
     */
    public LatLngBounds toBounds(LatLng center, double radius) {
        LatLng southwest = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 225);
        LatLng northeast = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 45);
        return new LatLngBounds(southwest, northeast);
    }


//region INDOORATLAS METHODS

    /**
     * Sets bitmap of floor plan as ground overlay on Google Maps
     *
     * Method based on IndoorAtlas examples from https://github.com/IndoorAtlas/android-sdk-examples with minor changes
     * Specifically https://github.com/IndoorAtlas/android-sdk-examples/blob/master/Basic/src/main/java/com/indooratlas/android/sdk/examples/mapsoverlay/MapsOverlayActivity.java
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
        }
    }

    /**
     * Download floor plan using Picasso library.
     *
     * Method based on IndoorAtlas examples from https://github.com/IndoorAtlas/android-sdk-examples with minor changes
     * Specifically https://github.com/IndoorAtlas/android-sdk-examples/blob/master/Basic/src/main/java/com/indooratlas/android/sdk/examples/mapsoverlay/MapsOverlayActivity.java
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
                    fetchMapSpinner.setVisibility(View.GONE); // hide loading spinner, map loaded successfully
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
                    fetchMapSpinner.setVisibility(View.GONE); // hide loading spinner, map load failed
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
     *
     * Method based on IndoorAtlas examples from https://github.com/IndoorAtlas/android-sdk-examples with minor changes
     * Specifically https://github.com/IndoorAtlas/android-sdk-examples/blob/master/Basic/src/main/java/com/indooratlas/android/sdk/examples/mapsoverlay/MapsOverlayActivity.java
     */
    private void fetchFloorPlan(String id) {
        fetchMapSpinner.setVisibility(View.VISIBLE); // show loading spinner, start of map loading

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
     *
     * Method based on IndoorAtlas examples from https://github.com/IndoorAtlas/android-sdk-examples with minor changes
     * Specifically https://github.com/IndoorAtlas/android-sdk-examples/blob/master/Basic/src/main/java/com/indooratlas/android/sdk/examples/mapsoverlay/MapsOverlayActivity.java
     */
    private void cancelPendingNetworkCalls() {
        if (mFetchFloorPlanTask != null && !mFetchFloorPlanTask.isCancelled()) {
            mFetchFloorPlanTask.cancel();
        }
    }

//endregion
//region MENU METHODS
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
            // logs user in, starts AuthenticationActivity
            case R.id.action_login:
                Intent intent = new Intent(this,AuthenticationActivity.class);
                startActivity(intent);
                break;
            // logs user out, removes verification-token from SharedPreferences and changes which button is shown
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
//endregion

    /**
     * Is run when positionlibrary.Api-class has completed its request to the API
     * @param jsonObject Returned data in JSON form
     * @param actionString Identifier string to recognize which request has returned a result
     */
    @Override
    public void onCompletedAction(JSONObject jsonObject, String actionString) {
        switch (actionString) {
            /* // TODO: remove Api.ALL_USERS
            case Api.ALL_USERS:
                //JSONObject dummyObject = jsonObject;
                break;
                */
            // API has returned all pathpoints
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
                    // todo: create string resource
                    showCustomToast(getApplicationContext(), "Exception when trying to fetch pathpoints from API: " + e.toString(), Toast.LENGTH_LONG);
                    e.printStackTrace();
                }
                break;

            default:
                break;
        }
    }

    /**
     * Is run if authorization failed
     */
    @Override
    public void onAuthorizationFailed() {
        //TODO: possible intent loop?
        SharedPreferences sharedPreferences = getSharedPreferences("AppPref",MODE_PRIVATE);

        if (sharedPreferences.getBoolean("LoggedInThisSession", true)) {
            Intent startAuthorization = new Intent(this, AuthenticationActivity.class);
            startActivity(startAuthorization);
        }
    }


    /**
     * Overrides onBackPressed, in order to prevent the app from exiting unless back was pressed twice within 5 seconds
     *
     * Based on http://stackoverflow.com/questions/14006461/android-confirm-app-exit-with-toast/18654014#18654014
     */
    @Override
    public void onBackPressed() {
        long currentTime = System.currentTimeMillis();
        if(currentTime - backPressedTimeStamp > 5000){
            showCustomToast(getApplicationContext(), getResources().getString(R.string.close_mapactivity_backpress_confirmation), Toast.LENGTH_SHORT);
            backPressedTimeStamp = currentTime;
        }
        else{
            super.onBackPressed();
        }
    }

    /**
     * Receives the coordinate returned from SearchResultsActivity if the user pressed the generate
     * path button for a person or location, then draws generates, segments and draws a path to that
     * location (if the user position has been set)
     * @param requestCode
     * @param resultCode
     * @param data
     */
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == SEARCH_RETURNED_COORDINATE_CODE) { // checks the request code
            if(resultCode == SEARCH_RETURNED_COORDINATE_RESULT) { // checks the result code
                targetPosition = new LatLng(data.getDoubleExtra("lat", 0), data.getDoubleExtra("lng", 0));
                targetFloor = (int) data.getDoubleExtra("floor", 1.0);

                // todo: remove toast
                showCustomToast(getApplicationContext(), targetPosition.toString() + ", " + targetFloor, Toast.LENGTH_SHORT);

                if (targetFloor != currentFloor) {
                    changeFloor(currentFloor);
                }

                path = requestPathFromPosLib(targetPosition, (int) targetFloor); // request path from positionlibrary
                if (path != null) {
                    if (path.size() != 0) {
                        fullSegmentedPath = generateFullSegmentedPath(path); // segments path

                        drawFloorPath(fullSegmentedPath); // draws path
                        mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(path.get(0).getLatitude(), path.get(0).getLongitude()), 19));
                    } else {
                        showCustomToast(getApplicationContext(), getResources().getString(R.string.path_not_found_exception), Toast.LENGTH_SHORT);
                    }
                }
            }
        }
    }

    /**
     * Requests positionlibrary to calculate a path from from the users current position/floor to
     * targetPosition/floor using Dijkstra's algorithm
     *
     * @param targetPosition LatLng indicating the position to generate a path to
     * @param targetFloor The floor the LatLng you are trying to generate a path to is on
     * @return The path to targetPosition in List<Point> form
     */
    public List<Point> requestPathFromPosLib(LatLng targetPosition, int targetFloor) {
        List<Point> pathList = new ArrayList<>();
        try {
            pathList = positionLibrary.wifiPosition.plotRoute(new Point(currentPosition.latitude, currentPosition.longitude, currentFloor), new Point(targetPosition.latitude, targetPosition.longitude, targetFloor), pathPointJson);
        } catch (PathNotFoundException ex) {
            showCustomToast(getApplicationContext(), getResources().getString(R.string.path_not_found_exception), Toast.LENGTH_SHORT);
        }

        return pathList;
    }

    /**
     * Segments a List<Point> (the path) by segmenting it into separate lists.
     * Every time the floor changes, a new list is started. Finally a ArrayList<List<Point>> is returned.
     * This segmentation is necessary to allow the drawing of paths which return to an already
     * visited floor. Ex. A path is drawn starting on the 1st floor, continues on the 2nd floor, and
     * then returns to a different point on the 1st floor. This would be 3 segments.
     *
     * @param pathList List<Point> containing the path calculated by positionlibrary
     * @return ArrayList<List<Point>> containing the generated segments (ArrayList<Point>)
     */
    public ArrayList<List<Point>> generateFullSegmentedPath(List<Point> pathList)
    {
        ArrayList<List<Point>> segmentedPathList = new ArrayList<List<Point>>();
        if (pathList != null) {
            // draw path for this floor if available
            int nextFloor = -100;
            List<Point> singleFloorPath = new ArrayList<Point>();
            segmentedPathList = new ArrayList<List<Point>>();

            for (int i = 0; i < pathList.size(); i++) { // iterate path
                singleFloorPath.add(pathList.get(i)); // add current point to singleFloorPath
                if (pathList.size() > (i + 1)) { // check if we are on the last point of the path
                    nextFloor = pathList.get(i + 1).getFloor(); // add the floor of the next point
                    if (nextFloor != pathList.get(i).getFloor()) { // if the next point is different than the current point
                        segmentedPathList.add(new ArrayList<Point>(singleFloorPath)); // add the list of points to the fullSegmentedPath list
                        singleFloorPath.clear(); // clear the current list
                    }
                } else {
                    segmentedPathList.add(singleFloorPath); // add the list of points to the fullSegmentedPath list
                }
            }
        }
        return segmentedPathList;
    }
}
