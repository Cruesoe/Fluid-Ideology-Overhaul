using System;
using FluidIdeologyOverhaul.Reform;
using UnityEngine;
using Verse;

namespace FluidIdeologyOverhaul.UI;

internal sealed class Dialog_ReformReview : Window
{
    private const float SectionGap = 18f;
    private readonly Action confirm;
    private readonly ReformSummary summary;
    private Vector2 scrollPosition;

    public override Vector2 InitialSize => new Vector2(720f, Mathf.Min(720f, Verse.UI.screenHeight));

    public Dialog_ReformReview(ReformSession session, Action confirm)
    {
        this.confirm = confirm;
        summary = ReformSummary.Build(session);
        forcePause = true;
        absorbInputAroundWindow = true;
        doCloseX = false;
        closeOnCancel = true;
    }

    public override void DoWindowContents(Rect inRect)
    {
        Text.Font = GameFont.Medium;
        Widgets.Label(new Rect(inRect.x, inRect.y, inRect.width, 35f), "FIO_ReviewTitle".Translate());
        Text.Font = GameFont.Small;

        Rect outRect = new Rect(inRect.x, inRect.y + 45f, inRect.width, inRect.height - 100f);
        float viewWidth = outRect.width - 16f;
        float contentHeight = CalculateContentHeight(viewWidth);
        Rect viewRect = new Rect(0f, 0f, viewWidth, Mathf.Max(outRect.height, contentHeight));

        Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
        float curY = 0f;
        DrawSection(ref curY, viewWidth, "FIO_PrimaryChange".Translate(), summary.PrimaryText);
        DrawSection(ref curY, viewWidth, "FIO_Consequences".Translate(), summary.ConsequenceText);
        DrawSection(ref curY, viewWidth, "FIO_Preserved".Translate(), summary.Preserved);
        if (summary.CosmeticChanges.Count > 0)
        {
            DrawSection(ref curY, viewWidth, "FIO_CosmeticChanges".Translate(), summary.CosmeticText);
        }

        curY += 6f;
        GUI.color = ColorLibrary.Yellow;
        DrawBody(ref curY, viewWidth, "FIO_ConsumesReform".Translate());
        GUI.color = Color.white;
        Widgets.EndScrollView();

        Rect backRect = new Rect(inRect.x, inRect.yMax - 40f, 180f, 40f);
        Rect confirmRect = new Rect(inRect.xMax - 180f, inRect.yMax - 40f, 180f, 40f);
        if (Widgets.ButtonText(backRect, "FIO_GoBack".Translate()))
        {
            Close();
        }
        if (Widgets.ButtonText(confirmRect, "FIO_ConfirmReform".Translate()))
        {
            Close();
            confirm();
        }
    }

    private float CalculateContentHeight(float width)
    {
        float height = SectionHeight(width, summary.PrimaryText)
            + SectionHeight(width, summary.ConsequenceText)
            + SectionHeight(width, summary.Preserved);
        if (summary.CosmeticChanges.Count > 0)
        {
            height += SectionHeight(width, summary.CosmeticText);
        }

        Text.Font = GameFont.Small;
        height += 6f + Text.CalcHeight("FIO_ConsumesReform".Translate(), width) + 12f;
        return height;
    }

    private static float SectionHeight(float width, string body)
    {
        Text.Font = GameFont.Medium;
        float headingHeight = Text.CalcHeight("Ag", width);
        Text.Font = GameFont.Small;
        float bodyHeight = Text.CalcHeight(body, width);
        return headingHeight + 6f + bodyHeight + SectionGap;
    }

    private static void DrawSection(ref float curY, float width, string heading, string body)
    {
        Text.Font = GameFont.Medium;
        float headingHeight = Text.CalcHeight(heading, width);
        Widgets.Label(new Rect(0f, curY, width, headingHeight), heading);
        curY += headingHeight + 6f;
        DrawBody(ref curY, width, body);
        curY += SectionGap;
    }

    private static void DrawBody(ref float curY, float width, string body)
    {
        Text.Font = GameFont.Small;
        float height = Text.CalcHeight(body, width);
        Widgets.Label(new Rect(0f, curY, width, height), body);
        curY += height;
    }
}
