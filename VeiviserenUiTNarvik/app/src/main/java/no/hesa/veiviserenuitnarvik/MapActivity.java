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
    private PositionLibrary positionLibrary = null;

    private BroadcastReceiver positionLibOutputReceiver = null;



    private Menu menuRef = null;

    private String currentFloorPlan;

    private LatLng currentPosition = null;
    private int currentFloor = -100;

    private LatLng targetPosition = null;
    private int targetFloor = -101;

    private ProgressBar fetchMapSpinner;
    private boolean positioningEnabled = false;

    private long backPressedTimeStamp;
    private Circle userPositionMarker;

    ArrayList<Polyline> polylineList = new ArrayList<Polyline>();
    ArrayList<LatLng> latLngList = new ArrayList<LatLng>();
    ArrayList<Circle> circleList = new ArrayList<Circle>();

    private int mapType;
    private String pathPointJson;

    private SearchView searchView;
    private Api api;
    private boolean pathPointsDownloading = false;

    private List<Point> path;
    private ArrayList<List<Point>> fullSegmentedPath;

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
        setSupportActionBar(toolbar); // TODO: crashes pre android 5.0

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

    @Override
    public void onMapReady(GoogleMap googleMap) {
        mMap = googleMap;

        // retrieve various sharedPreferences
        SharedPreferences sharedPreferences = getSharedPreferences("MapActivityPrefs",MODE_PRIVATE);

        mapType = sharedPreferences.getInt("MapType", googleMap.MAP_TYPE_NONE);
        mMap.setMapType(mapType);

        float zoomLevel = sharedPreferences.getFloat("ZoomLevel", UIT_NARVIK_ZOOMLEVEL);
        float lat = sharedPreferences.getFloat("CurrentLat", (float)UIT_NARVIK_POSITION.latitude);
        float lng = sharedPreferences.getFloat("CurrentLng", (float)UIT_NARVIK_POSITION.longitude);
        currentFloor = sharedPreferences.getInt("CurrentFloor", UIT_NARVIK_DEFAULTFLOOR);
        currentPosition = new LatLng(lat, lng);

        pathPointJson = sharedPreferences.getString("PathPointJson", null);

        positioningEnabled = sharedPreferences.getBoolean("PositioningEnabled", false);
        enablePositioning(positioningEnabled, false);

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
     * Colors first and last circle differently.
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

    private void drawUserPosition()
    {
        clearDrawnUserPosition();

        CircleOptions co = new CircleOptions()
                .center(currentPosition)
                .radius(1)
                .strokeColor(getResources().getColor(R.color.route_circle_end_border_color))
                .fillColor(getResources().getColor(R.color.route_circle_end_color))
                .zIndex(0.50f);

        userPositionMarker = mMap.addCircle(co);
    }

    private void clearDrawnUserPosition()
    {
        if (userPositionMarker != null) {
            userPositionMarker.remove();
        }
    }
//endregion

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
    //// TODO: 28/04/2016 hardcoded floor values when changing floor with buttons 
    private void registerButtonListeners()
    {
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
                mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(UIT_NARVIK_POSITION, UIT_NARVIK_ZOOMLEVEL));
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
        FloatingActionButton fab = (FloatingActionButton) findViewById(R.id.fab_automatic_positioning);
        SharedPreferences.Editor sharedPreferences = getSharedPreferences("MapActivityPrefs", MODE_PRIVATE).edit();
        if (!positioningEnabled) // turn on
        {
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

    private void registerOnMapClickReceiver()
    {
        // adds a marker
        mMap.setOnMapClickListener(new GoogleMap.OnMapClickListener() {
            @Override
            public void onMapClick(LatLng point) {
                // TODO: remove marker for launch)
                if (mMarker != null) {
                    mMarker.remove();
                }
                // new CircleOptions().center(point).radius(3.0).fillColor(R.color.radiusfillcolor).strokeColor(R.color.radiusstrokecolor).strokeWidth(2)
                mMarker = mMap.addMarker(new MarkerOptions().position(point).title("Lat: " + point.latitude + " Lng: " + point.longitude));
                mMarker.setDraggable(true);
                mMarker.showInfoWindow();
                currentPosition = new LatLng(point.latitude, point.longitude);
//                fullSegmentedPath = null;
                clearDrawnPaths();
                drawUserPosition();
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

                        if (floor != currentFloor) {
                            changeFloor(floor);
                            clearDrawnPaths();
                        }

                        clearDrawnUserPosition();

                        currentPosition = latLng;
                        currentFloor = floor;

                        if (targetPosition != null && targetFloor > -100) {
                            path = requestPathFromPosLib(targetPosition, targetFloor);
                            fullSegmentedPath = generateFullSegmentedPath(path);
                        }

                        if (fullSegmentedPath != null) {
                            drawFloorPath(fullSegmentedPath);
                        }
                        else {
                            drawUserPosition();
                        }
                        if (path != null) {
                            Point currentPoint = new Point(latLng.latitude, latLng.longitude, floor);
                            if (arrivedAtFinalLocation(currentPoint, path.get(path.size() - 1))) //gets final point
                            {
                                // unbind targetPosition etc etc
                            }
                        }

                        //TODO: set to current zoom level, but have the first update zoom to 19
                        mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(currentPosition, 19));
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
    public LatLngBounds toBounds(LatLng center, double radius) {
        LatLng southwest = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 225);
        LatLng northeast = SphericalUtil.computeOffset(center, radius * Math.sqrt(2.0), 45);
        return new LatLngBounds(southwest, northeast);
    }


//region INDOORATLAS METHODS
    // TODO: GIVE CREDIT! important.
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
//endregion

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
        if(currentTime - backPressedTimeStamp > 5000){
            showCustomToast(getApplicationContext(), getResources().getString(R.string.close_mapactivity_backpress_confirmation), Toast.LENGTH_SHORT);
            backPressedTimeStamp = currentTime;
        }
        else{
            super.onBackPressed();
        }
    }

    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        super.onActivityResult(requestCode, resultCode, data);
        if (requestCode == SEARCH_RETURNED_COORDINATE_CODE) {
            if(resultCode == SEARCH_RETURNED_COORDINATE_RESULT) {
                targetPosition = new LatLng(data.getDoubleExtra("lat", 0), data.getDoubleExtra("lng", 0));
                targetFloor = (int) data.getDoubleExtra("floor", 1.0);

                showCustomToast(getApplicationContext(), targetPosition.toString() + ", " + targetFloor, Toast.LENGTH_SHORT);

                if (targetFloor != currentFloor) {
                    changeFloor(currentFloor);
                }

                path = requestPathFromPosLib(targetPosition, (int) targetFloor);
                fullSegmentedPath = generateFullSegmentedPath(path);

                drawFloorPath(fullSegmentedPath);
                mMap.animateCamera(CameraUpdateFactory.newLatLngZoom(new LatLng(path.get(0).getLatitude(), path.get(0).getLongitude()), 19));
            }
        }
    }

    public List<Point> requestPathFromPosLib(LatLng targetPosition, int targetFloor) {
        List<Point> pathList = new ArrayList<>();
        try {
            pathList = positionLibrary.wifiPosition.plotRoute(new Point(currentPosition.latitude, currentPosition.longitude, currentFloor), new Point(targetPosition.latitude, targetPosition.longitude, targetFloor), pathPointJson);
        } catch (PathNotFoundException ex) {
            showCustomToast(getApplicationContext(), getResources().getString(R.string.path_not_found_exception), Toast.LENGTH_SHORT);
        }

        return pathList;
    }

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
