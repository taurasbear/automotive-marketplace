import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";
import type { HubConnection } from "@microsoft/signalr";

export function useDashboardHub(connection: HubConnection | null | undefined) {
  const queryClient = useQueryClient();

  useEffect(() => {
    if (!connection || connection.state !== HubConnectionState.Connected) return;

    const handler = () => {
      queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] });
    };

    connection.on("DashboardUpdated", handler);
    return () => {
      connection.off("DashboardUpdated", handler);
    };
  }, [connection, queryClient]);
}