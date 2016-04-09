package no.hesa.veiviserenuitnarvik;


import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;


/**
 * A simple {@link Fragment} subclass.
 */
public class MapsFragment extends Fragment {

    private BlueDotView mImageView;

    public MapsFragment() {
        // Required empty public constructor
    }


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View view = inflater.inflate(R.layout.activity_main, container, false);
        mImageView = (BlueDotView) view.findViewById(R.id.blue_dot_view);
        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_maps, container, false);
    }


    public void setRadius(float f)
    {
        mImageView.setRadius(f);
    }

    public void setImage(com.davemorrissey.labs.subscaleview.ImageSource image)
    {
        mImageView.setImage(image);
    }

}
