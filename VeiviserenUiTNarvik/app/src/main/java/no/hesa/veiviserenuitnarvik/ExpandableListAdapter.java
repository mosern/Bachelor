package no.hesa.veiviserenuitnarvik;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Typeface;

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

public class ExpandableListAdapter extends BaseExpandableListAdapter {

    private Context _context;
    private List<String> _listDataHeader; // header titles
    // child data in format of header title, child title
    private HashMap<String, List<? extends Object>> _listDataChild;
    private List<String> passedClass;
    private String icon;

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

    @Override
    public View getChildView(final int groupPosition, final int childPosition, boolean isLastChild, View convertView, ViewGroup parent) {
        Object child = getChild(groupPosition, childPosition);


        if (convertView == null) {
            LayoutInflater infalInflater = (LayoutInflater) this._context
                    .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            convertView = infalInflater.inflate(R.layout.search_results_child_item, null);
        }




        LayoutInflater infalInflater = (LayoutInflater) this._context
                .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
        View rowView = infalInflater.inflate(R.layout.search_results_listroot, parent, false);


        String childText = "";
        if (passedClass != null) {
            if (passedClass.get(groupPosition).compareTo("person") == 0) {
                final Person person;
                person = (Person) child;
                icon = "person";
//                ImageView image = (ImageView) rowView.findViewById(R.id.iv_listroot_icon);
//                if (image != null) {
//                    image.setImageResource(R.drawable.ic_person_white_24dp);
//                }
//                else {
//                    Toast.makeText(_context, person.getName() + " was null", Toast.LENGTH_SHORT).show();
//                }

                childText = person.getName() + "\n" + person.getLocation().getLocNr() + "\n" + person.getEmail();
                convertView = setupPersonButtons(convertView, groupPosition, person);
            }

            if (passedClass.get(groupPosition).compareTo("location") == 0) {
                final Location location;
                location = (Location) child;
                icon = "location";
//                ImageView image = (ImageView) rowView.findViewById(R.id.iv_listroot_icon);
//                if (image != null) {
//                    image.setImageResource(R.drawable.ic_gps_not_fixed_white_24dp);
//                }
//                else {
//                    Toast.makeText(_context, location.getLocNr() + " was null", Toast.LENGTH_SHORT).show();
//                }

                childText = location.getLocNr();
                convertView = setupLocationButton(convertView, groupPosition, location);
            }
        }

        TextView txtListChild = (TextView) convertView.findViewById(R.id.tv_child_info);
        txtListChild.setText(childText);

        return convertView;
    }

    public View setupLocationButton(View convertView, final int groupPosition, final Location location)
    {
        ImageButton smsButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_sms);
        ImageButton tlfMobileButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_tlfMobile);
        ImageButton tlfOfficeButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_tlfOffice);
        ImageButton emailButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_email);
        ImageButton routeButton = (ImageButton)convertView.findViewById(R.id.btn_child_item_routeto);


        // ROUTE
        routeButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                try {
                    // REDIRECT WITH ROUTE HERE
                    //TODO: change Location to Point?
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

    public View setupPersonButtons(View convertView, final int groupPosition, final Person person)
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
                    PositionLibrary positionLibrary = new PositionLibrary();
                    //positionLibrary.wifiPosition.plotRoute();
                }
                catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });

        return convertView;
    }

    private void sendLocationDestination(final Location location )
    {
        Intent intent = new Intent(_context, MapActivity.class);
        intent.setAction("no.hesa.veiviserennarvik.LAT_LNG_RETURN");
        intent.putExtra("lat", location.getCoordinate().getLat());
        intent.putExtra("lng", location.getCoordinate().getLng());
        intent.putExtra("floor", location.getCoordinate().getAlt());
        _context.startActivity(intent);
//        ((Activity) _context).finish();
    }

    private void sendPersonDestination(final Person person)
    {
        Intent intent = new Intent(_context,MapActivity.class);
        intent.setAction("no.hesa.veiviserennarvik.LAT_LNG_RETURN");
        intent.putExtra("lat", person.getLocation().getCoordinate().getLat());
        intent.putExtra("lng", person.getLocation().getCoordinate().getLng());
        intent.putExtra("floor", person.getLocation().getCoordinate().getAlt());
        _context.startActivity(intent);
    }

//region SMS/CALL/EMAIL
    private void sendSMS(String tlfNumber)
    {
        Intent intent = new Intent(Intent.ACTION_VIEW);
        intent.setData(Uri.parse("smsto:"));
        intent.putExtra("address", new String(tlfNumber));
        //intent.putExtra("sms_body", "");
     //   intent.setType("vnd.android-dir/mms-sms");
        try {
            _context.startActivity(Intent.createChooser(intent, _context.getString(R.string.sms_selection_descriptor)));
        } catch (android.content.ActivityNotFoundException ex) {
            Toast.makeText(_context, _context.getString(R.string.sms_no_sms_apps_installed), Toast.LENGTH_SHORT).show();
        }
    }

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

    private void sendEmail(String emailAddress)
    {
        Intent intent = new Intent(Intent.ACTION_SEND);
        // intent.setType("text/plain");
        intent.setType("message/rfc822");
        intent.putExtra(Intent.EXTRA_EMAIL, new String[]{emailAddress});
        //intent.putExtra(Intent.EXTRA_SUBJECT, "Subject");
        //intent.putExtra(Intent.EXTRA_TEXT, "I'm email body.");

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

    @Override
    public View getGroupView(int groupPosition, boolean isExpanded, View convertView, ViewGroup parent) {
        String headerTitle = (String) getGroup(groupPosition);
        if (convertView == null) {
            LayoutInflater infalInflater = (LayoutInflater) this._context
                    .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            convertView = infalInflater.inflate(R.layout.search_results_listroot, null);
        }

        ImageView image = (ImageView) convertView.findViewById(R.id.iv_listroot_icon);
        if (image != null) {
            if (passedClass.get(groupPosition).compareTo("person") == 0) {
                image.setImageResource(R.drawable.ic_person_white_24dp);
            }

            if (passedClass.get(groupPosition).compareTo("location") == 0) {
                image.setImageResource(R.drawable.ic_gps_not_fixed_white_24dp);
            }
        }

        TextView lblListHeader = (TextView) convertView.findViewById(R.id.tv_list_header);
        //lblListHeader.setTypeface(null, Typeface.BOLD);
        lblListHeader.setText(headerTitle);

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
