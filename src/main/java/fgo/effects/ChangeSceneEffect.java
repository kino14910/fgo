package fgo.effects;

import com.badlogic.gdx.graphics.Color;
import com.badlogic.gdx.graphics.Texture;
import com.badlogic.gdx.graphics.g2d.SpriteBatch;
import com.megacrit.cardcrawl.core.Settings;

public class ChangeSceneEffect extends AbstractChangeSceneEffect {
    private final Texture img;
    public float x;
    public Color color;
    
    public ChangeSceneEffect(Texture img) {
        color = Color.WHITE.cpy();
        renderBehind = true;
        this.img = img;
        x = -Settings.WIDTH * 1.7F;
    }

    @Override
    public void update() {}

    @Override
    public void end() {
        x = Settings.WIDTH;
    }

    @Override
    public void render(SpriteBatch sb) {
        sb.flush();
        sb.setColor(Color.WHITE.toFloatBits());

        // 计算图像的宽度和高度
        float drawWidth = Settings.WIDTH;
        float drawHeight = (float) Settings.WIDTH / img.getWidth() * img.getHeight();

        // 如果高等比缩放后小于屏幕高度，则将高度设置为屏幕高度，并相应地调整宽度
        if (drawHeight < Settings.HEIGHT) {
            drawHeight = Settings.HEIGHT;
            drawWidth = (float) Settings.HEIGHT / img.getHeight() * img.getWidth();
        }

        sb.draw(img, 0.0f, 0.0f, drawWidth, drawHeight);
    }

    @Override
    public void dispose() {}
}