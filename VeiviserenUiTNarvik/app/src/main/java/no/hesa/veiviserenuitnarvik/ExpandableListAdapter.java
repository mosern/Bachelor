package no.hesa.veiviserenuitnarvik;

import android.app.Activity;
import android.content.Intent;

import java.util.HashMap;
import java.util.List;

import android.content.Context;
import android.net.Uri;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseExpandableListAdapter;
import android.widget.ImageButton;
import android.widget.ImageView;
import android.widget.TextView;
import android.widget.Toast;

import no.hesa.positionlibrary.PositionLibrary;
import no.hesa.veiviserenuitnarvik.dataclasses.Location;
import no.hesa.veiviserenuitnarvik.dataclasses.Person;

/**
 * ExpandableListAdapter. Receives a list of headers and a list of child objects and which header
 * they belong under, populates them with content (text, buttons, icons) and handles all clicking,
 * expanding and general functions of an expandable listview.
 *
 * Contains functions for each child to start call, sms and email applications, as well return the
 * geolocation of a child (which is either a Person associated with a Location, or a simple Location)
 * to the starting activity.
 *
 * Based on // http://www.androidhive.info/2013/07/android-expandable-list-view-tutorial/ which
 * mostly sets up the general class structure (overridden methods required by BaseExpandableListAdapter etc).
 * It has been greatly expanded and made generic.
 */
public class ExpandableListAdapter extends BaseExpandableListAdapter {

    private Context _context;
    private List<String> _listDataHeader; // header titles
    private HashMap<String, List<? extends Object>> _listDataChild; // map of listheader + child-object
    private List<String> passedClass; // keeps track of what class was passed to the Expandable ListAdapter in order so they can be retrieved now that they are generic

    public ExpandableListAdapter(Context context, List<String> listDataHeader, HashMap<String, List<? extends Object>> listChildData, List<String> passedClass) {
        this._context = context;
        this._listDataHeader = listDataHeader;
        this._listDataChild = listChildData;
        this.passedClass = passedClass;
    }

    @Override
    public Object getChild(int groupPosition, int childPosititon) {
        return this._listDataChild.get(this._listDataHeader.get(groupPosition))
                .get(childPosititon);
    }

    @Override
    public long getChildId(int groupPosition, int childPosition) {
        return childPosition;
    }

    /**
     * Inflates the view of a specific child item (search_results_child_item.xml),
     * fetches the child, determines what type of obejct it is, the populates the view with
     * text and buttons belonging to that child
     *
     * @param groupPosition Header position in list
     * @param childPosition Child position in list
     * @param isLastChild Is true if the child is the last child in the group
     * @param convertView Child view
     * @param parent Surrounding ViewGroup
     * @return Returns the child view
     */
    @Override
    public View getChildView(final int groupPosition, final int childPosition, boolean isLastChild, View convertView, ViewGroup parent) {
        Object child = getChild(groupPosition, childPosition);

        if (convertView == null) {
            LayoutInflater infalInflater = (LayoutInflater) this._context
                    .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            convertView = infalInflater.inflate(R.layout.search_results_child_item, null);
        }

        String childText = "";
        if (passedClass != null) { // check if passedClass is null to prevent possible crash
            if (passedClass.get(groupPosition).compareTo("person") == 0) { // checks what type of object is passed
                final Person person;
                person = (Person) child;
                // todo: customize show text in textview
                // set text displayed in child body
                childText = person.getName() + "\n" + person.getLocation().getLocNr() + "\n" + person.getEmail();
                // set up buttons/register listeners for child, and add them to the view
                convertView = setupPersonButtons(convertView, person);
            }

            if (passedClass.get(groupPosition).compareTo("location") == 0) {
                final Location location;
                location = (Location) child;
                // todo: customize show text in textview
                // set text displayed in child body
                childText = location.getLocNr();
                // set up buttons/register listeners for child, and add them to the view
                convertView = setupLocationButton(convertView, location);
            }
        }

        TextView txtListChild = (TextView) convertView.findViewById(R.id.tv_child_info);
        txtListChild.setText(childText);
        return convertView;
    }

    /**
     * Sets up button-clicklisteners (and hides unnecessary buttons) in for Location type child-views
     *
     * @param convertView View
     * @param location Location object
     * @return Child-view populated with buttons
     */
    public View setupLocationButton(View convertView, final Location location)
    {
        ImageButton smsButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_sms);
        ImageButton tlfMobileButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_tlfMobile);
        ImageButton tlfOfficeButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_tlfOffice);
        ImageButton emailButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_email);
        ImageButton routeButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_routeto);

        // ROUTE
        routeButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                try {
                    // REDIRECT WITH ROUTE HERE
                    sendLocationDestination(location);
                    //PositionLibrary positionLibrary = new PositionLibrary();
                    //positionLibrary.wifiPosition.plotRoute(location);
                }
                catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });

        smsButton.setVisibility(View.INVISIBLE);
        tlfMobileButton.setVisibility(View.INVISIBLE);
        tlfOfficeButton.setVisibility(View.INVISIBLE);
        emailButton.setVisibility(View.INVISIBLE);

        return convertView;
    }

    /**
     * Sets up button-clicklisteners in for Person type child-views
     *
     * @param convertView
     * @param person
     * @return Child-view populated with buttons
     */
    public View setupPersonButtons(View convertView, final Person person)
    {
        ImageButton smsButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_sms);
        ImageButton tlfMobileButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_tlfMobile);
        ImageButton tlfOfficeButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_tlfOffice);
        ImageButton emailButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_email);
        ImageButton routeButton = (ImageButton)convertView.findViewById(R.id.btn_child_item_routeto);

        // SMS
        smsButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                try {
                    // SEND SMS HERE
                    sendSMS(person.getTlfMobile());
                } catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });
        smsButton.setVisibility(View.VISIBLE);

        // TLFMOBILE
        tlfMobileButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                try {
                    // CALL MOBILE HERE
                    callNumber(person.getTlfMobile());
                } catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });
        tlfMobileButton.setVisibility(View.VISIBLE);

        // TLFOFFICE
        tlfOfficeButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                try {
                    // CALL OFFICE HERE
                    callNumber(person.getTlfOffice());
                } catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });
        tlfOfficeButton.setVisibility(View.VISIBLE);

        // EMAIL
        emailButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                try {
                    // SEND EMAIL HERE
                    sendEmail(person.getEmail());
                } catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });
        emailButton.setVisibility(View.VISIBLE);

        // ROUTE
        routeButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                try {
                    // REDIRECT WITH ROUTE HERE
                    sendPersonDestination(person);
                    //PositionLibrary positionLibrary = new PositionLibrary();
                    //positionLibrary.wifiPosition.plotRoute();
                }
                catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });

        return convertView;
    }

    /**
     * Returns the coordinate of a Location to MapActivity and closes SearchResultActivity
     * if the child views button to generate route was pressed
     *
     * @param location Location object for that specific child
     */
    private void sendLocationDestination(final Location location )
    {
        Intent intent = new Intent(_context, MapActivity.class);
        intent.setAction("no.hesa.veiviserennarvik.LAT_LNG_RETURN");
        intent.putExtra("lat", location.getCoordinate().getLat());
        intent.putExtra("lng", location.getCoordinate().getLng());
        intent.putExtra("floor", location.getCoordinate().getAlt());
        ((Activity)_context).setResult(MapActivity.SEARCH_RETURNED_COORDINATE_RESULT, intent);
        ((Activity)_context).finish();
    }

    /**
     * Returns the coordinate of a Person to MapActivity and closes SearchResultActivity
     * if the child views button to generate route was pressed
     *
     * @param person Location object for that specific child
     */
    private void sendPersonDestination(final Person person)
    {
        Intent intent = new Intent(_context,MapActivity.class);
        intent.setAction("no.hesa.veiviserennarvik.LAT_LNG_RETURN");
        intent.putExtra("lat", person.getLocation().getCoordinate().getLat());
        intent.putExtra("lng", person.getLocation().getCoordinate().getLng());
        intent.putExtra("floor", person.getLocation().getCoordinate().getAlt());
        ((Activity)_context).setResult(MapActivity.SEARCH_RETURNED_COORDINATE_RESULT, intent);
        ((Activity)_context).finish();
    }

//region SMS/CALL/EMAIL

    /**
     * Opens an SMS application chooser
     *
     * @param tlfNumber Telephone number SMS application will use
     */
    private void sendSMS(String tlfNumber)
    {
        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.setData(Uri.parse("smsto:"));
        intent.putExtra("address", new String(tlfNumber));
        try {
            _context.startActivity(Intent.createChooser(intent, _context.getString(R.string.sms_selection_descriptor)));
        } catch (android.content.ActivityNotFoundException ex) {
            Toast.makeText(_context, _context.getString(R.string.sms_no_sms_apps_installed), Toast.LENGTH_SHORT).show();
        }
    }

    /**
     * Opens an Call application chooser
     *
     * @param tlfNumber Telephone number calling application will use
     */
    private void callNumber(String tlfNumber)
    {
        Intent intent = new Intent(Intent.ACTION_CALL);
        intent.setData(Uri.parse("tel:" + tlfNumber));

        try {
            _context.startActivity(Intent.createChooser(intent, _context.getString(R.string.call_selection_descriptor)));
        } catch (android.content.ActivityNotFoundException ex) {
            Toast.makeText(_context, _context.getString(R.string.call_no_calling_apps_installed), Toast.LENGTH_SHORT).show();
        }
    }

    /**
     * Opens an email application chooser
     *
     * @param emailAddress Email-address email application will use
     */
    private void sendEmail(String emailAddress)
    {
        Intent intent = new Intent(Intent.ACTION_SEND);
        intent.setType("message/rfc822");
        intent.putExtra(Intent.EXTRA_EMAIL, new String[]{emailAddress});

        try {
            _context.startActivity(Intent.createChooser(intent, _context.getString(R.string.email_selection_descriptor)));
        } catch (android.content.ActivityNotFoundException ex) {
            Toast.makeText(_context, _context.getString(R.string.email_no_email_apps_installed), Toast.LENGTH_SHORT).show();
        }
    }
//endregion

    @Override
    public int getChildrenCount(int groupPosition) {
        return this._listDataChild.get(this._listDataHeader.get(groupPosition))
                .size();
    }

    @Override
    public Object getGroup(int groupPosition) {
        return this._listDataHeader.get(groupPosition);
    }

    @Override
    public int getGroupCount() {
        return this._listDataHeader.size();
    }

    @Override
    public long getGroupId(int groupPosition) {
        return groupPosition;
    }

    /**
     * Inflates the view of a header item (search_results_listroot.xml),
     * determines what type of object it is and adds a corresponding icon to the header
     *
     * @param groupPosition Header position in list
     * @param isExpanded True is viewgroup is expanded
     * @param convertView View
     * @param parent Surrounding ViewGroup
     * @return View
     */
    @Override
    public View getGroupView(int groupPosition, boolean isExpanded, View convertView, ViewGroup parent) {
        String headerTitle = (String) getGroup(groupPosition);

        if (convertView == null) {
            LayoutInflater infalInflater = (LayoutInflater) this._context
                    .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            convertView = infalInflater.inflate(R.layout.search_results_listroot, null);
        }

        // applies corresponding icon to the header
        ImageView image = (ImageView) convertView.findViewById(R.id.iv_listroot_icon);
        if (image != null) {
            if (passedClass.get(groupPosition).compareTo("person") == 0) {
                image.setImageResource(R.drawable.ic_person_white_24dp);
            }

            if (passedClass.get(groupPosition).compareTo("location") == 0) {
                image.setImageResource(R.drawable.ic_gps_not_fixed_white_24dp);
            }
        }

        TextView tvListHeader = (TextView) convertView.findViewById(R.id.tv_list_header);
        tvListHeader.setText(headerTitle);

        return convertView;
    }

    @Override
    public boolean hasStableIds() {
        return false;
    }

    @Override
    public boolean isChildSelectable(int groupPosition, int childPosition) {
        return true;
    }
}
