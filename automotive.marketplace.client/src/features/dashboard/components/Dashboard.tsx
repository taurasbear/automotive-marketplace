import { useChatHub } from "@/features/chat";
import { formatCurrency } from "@/lib/i18n/formatNumber";
import { useQuery } from "@tanstack/react-query";
import { Calendar, CircleDollarSign, Clock, FileText } from "lucide-react";
import { useTranslation } from "react-i18next";
import { getDashboardSummaryOptions } from "../api/getDashboardSummaryOptions";
import { useDashboardHub } from "../api/useDashboardHub";
import { DashboardTile } from "./DashboardTile";

export function Dashboard() {
  const { t } = useTranslation("dashboard");
  const { connection } = useChatHub();
  const { data, isLoading } = useQuery(getDashboardSummaryOptions);

  useDashboardHub(connection);

  if (isLoading) {
    return (
      <div className="mx-auto max-w-3xl animate-pulse space-y-3 py-4">
        <div className="bg-muted h-4 w-40 rounded" />
        <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
          {Array.from({ length: 4 }).map((_, i) => (
            <div key={i} className="bg-muted h-24 rounded-lg" />
          ))}
        </div>
      </div>
    );
  }

  if (!data) return null;

  const totalActions =
    data.offers.pendingCount +
    data.meetings.upcomingCount +
    data.contracts.actionNeededCount +
    data.availability.pendingCount;

  if (totalActions === 0) return null;

  return (
    <div className="mx-auto max-w-3xl py-4">
      <div className="grid grid-cols-2 gap-3 md:grid-cols-4">
        <DashboardTile
          icon={<CircleDollarSign className="h-5 w-5" />}
          title={t("offers.title")}
          count={data.offers.pendingCount}
          subtitle={
            data.offers.pendingCount > 0
              ? t("offers.awaiting", { count: data.offers.pendingCount })
              : t("offers.none")
          }
          detail={
            data.offers.newestOfferAmount
              ? `${formatCurrency(data.offers.newestOfferAmount)} — ${data.offers.newestOfferFrom}`
              : ""
          }
          isHighlighted={data.offers.pendingCount > 0}
        />
        <DashboardTile
          icon={<Calendar className="h-5 w-5" />}
          title={t("meetings.title")}
          count={data.meetings.upcomingCount}
          subtitle={
            data.meetings.nextMeetingAt
              ? t("meetings.next", {
                  date: new Date(
                    data.meetings.nextMeetingAt,
                  ).toLocaleDateString(),
                })
              : t("meetings.none")
          }
          detail={data.meetings.nextMeetingListing ?? ""}
        />
        <DashboardTile
          icon={<FileText className="h-5 w-5" />}
          title={t("contracts.title")}
          count={data.contracts.actionNeededCount}
          subtitle={
            data.contracts.actionNeededCount > 0
              ? t("contracts.needsAction", {
                  count: data.contracts.actionNeededCount,
                })
              : t("contracts.none")
          }
          detail={data.contracts.nextActionListing ?? ""}
        />
        <DashboardTile
          icon={<Clock className="h-5 w-5" />}
          title={t("availability.title")}
          count={data.availability.pendingCount}
          subtitle={
            data.availability.pendingCount > 0
              ? t("availability.pending", {
                  count: data.availability.pendingCount,
                })
              : t("availability.none")
          }
          detail=""
        />
      </div>
    </div>
  );
}
