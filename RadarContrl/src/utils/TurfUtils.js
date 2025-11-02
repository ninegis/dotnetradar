import * as turf from '@turf/turf'
import {area, center, featureCollection, point, polygon} from "@turf/turf";
import {c32lonlat} from "@/utils/tools.js";
export class TurfUtils{
    /**params
     * 计算两点间的角度
     * @param pt1 //[经度,纬度]
     * @param pt2 //[经度,纬度]
     * @constructor
     */
    static ComputeAngleByTwoPoint(pt1,pt2){
        const point1 = turf.point(pt1);
        const point2 = turf.point(pt2);
        return turf.bearing(point1, point2);
    }
    static ComputeAreaByPoints(pts){
        let currentpolygon = polygon([pts]);
        let currentarea = area(currentpolygon);
        if (currentarea>=1000000){
            currentarea = (currentarea/1000000).toFixed(4)+'平方公里';
        }else{
            currentarea = currentarea.toFixed(4)+'平方米';
        }
        return currentarea;
    }
    static ComputeCenterByPoints(pts){
            const arr = [];
            for (let i = 0; i < pts.length; i++) {
                if (pts[i].length===3){
                    arr.push(point([pts[i][0],pts[i][1]]));
                }else{
                    arr.push(point(pts[i]));
                }
            }
            return center(featureCollection(arr));
    }
}