using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GeoNameAddress.Models
{
    public class Point
    {
        double _x, _y;
        public Point(double x, double y)
        {
            _x = x;
            _y = y;
        }
        public Point()
        {
        }
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }
    }
}