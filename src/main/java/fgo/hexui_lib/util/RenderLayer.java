package fgo.hexui_lib.util;

public class RenderLayer {
    public static final boolean[] CM_FULL = {true, true, true, true};
    public static final boolean[] CM_ALPHA = {false, false, false, true};

    public enum BLENDMODE {
        NORMAL,
        SCREEN,
        LINEAR_DODGE,
        MULTIPLY,
        CREATEMASK,
        RECEIVEMASK
    }
}