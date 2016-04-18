package no.hesa.veiviserenuitnarvik;

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
import android.widget.TextView;
import android.widget.Toast;

import no.hesa.veiviserenuitnarvik.dataclasses.Location;
import no.hesa.veiviserenuitnarvik.dataclasses.Person;

public class ExpandableListAdapter extends BaseExpandableListAdapter {

    private Context _context;
    private List<String> _listDataHeader; // header titles
    // child data in format of header title, child title
    private HashMap<String, List<? extends Object>> _listDataChild;
    private List<String> passedClass;

    Person person;
    Location location;

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

        String childText = "";
        if (passedClass.get(groupPosition) == "person")
        {
            person = (Person)child;
            childText = person.getName() + "\n" + person.getEmail();
        }

        if (passedClass.get(groupPosition) == "location")
        {
            location = (Location)child;
            childText = location.getName() + "\n" + location.getLocNr() + "\n" + location.getType().getName();
        }

        if (convertView == null) {
            LayoutInflater infalInflater = (LayoutInflater) this._context
                    .getSystemService(Context.LAYOUT_INFLATER_SERVICE);
            convertView = infalInflater.inflate(R.layout.search_results_child_item, null);
        }

        TextView txtListChild = (TextView) convertView.findViewById(R.id.tv_child_info);
        txtListChild.setText(childText);

        convertView = setupButtons(convertView, groupPosition);

        return convertView;
    }

    public View setupButtons(View convertView, final int groupPosition)
    {
        ImageButton smsButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_sms);
        ImageButton tlfMobileButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_tlfMobile);
        ImageButton tlfOfficeButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_tlfOffice);
        ImageButton emailButton = (ImageButton) convertView.findViewById(R.id.btn_child_item_email);
        ImageButton routeButton = (ImageButton)convertView.findViewById(R.id.btn_child_item_routeto);

        if (passedClass.get(groupPosition) == "person") {
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
        }
        else
        {
            smsButton.setVisibility(View.INVISIBLE);
            tlfMobileButton.setVisibility(View.INVISIBLE);
            tlfOfficeButton.setVisibility(View.INVISIBLE);
            emailButton.setVisibility(View.INVISIBLE);
        }

        // ROUTE
        routeButton.setOnClickListener(new View.OnClickListener() {

            @Override
            public void onClick(View v) {
                try {
                    // REDIRECT WITH ROUTE HERE
                    redirectWithRoute(groupPosition);
                }
                catch (Exception ex) {
                    ex.printStackTrace();
                }
            }
        });

        return convertView;
    }

    private void redirectWithRoute(int groupPosision)
    {
        Intent intent = new Intent(_context,MapActivity.class);
        intent.setAction("LAT_LNG_RETURN");
        if(passedClass.get(groupPosision) == "location") {
            intent.putExtra("lat", location.getCoordinate().getLat());
            intent.putExtra("lng", location.getCoordinate().getLng());
        }
        /*
        if(passedClass.get(groupPosision) == "person") {
            intent.putExtra("lat", person.getCoordinate().getLat());
            intent.putExtra("lng", location.getCoordinate().getLng());
        }
        */
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

        TextView lblListHeader = (TextView) convertView.findViewById(R.id.lblListHeader);
        lblListHeader.setTypeface(null, Typeface.BOLD);
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
