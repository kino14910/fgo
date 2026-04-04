package fgo.hexui_lib.util;

public class RenderCommandLayer extends RenderLayer {
    public enum COMMAND {
        FBO_START,
        FBO_END,
        FBO_END_SCREEN,
        FBO_END_DOUBLESCREEN,
        ATTEMPT_RESET
    }
    
    public COMMAND command;

    public RenderCommandLayer(COMMAND command) {
        this.command = command;
    }
}
