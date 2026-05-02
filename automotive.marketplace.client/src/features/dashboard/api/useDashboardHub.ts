import { chatConnectionRef } from "@/features/chat/api/useChatHub";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";
import { HubConnectionState } from "@microsoft/signalr";

export function useDashboardHub() {
  const queryClient = useQueryClient();

  useEffect(() => {
    const checkAndSubscribe = () => {
      const conn = chatConnectionRef.current;
      if (!conn || conn.state !== HubConnectionState.Connected) return;
      
      const handler = () => {
        queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] });
      };
      
      conn.on("DashboardUpdated", handler);
      return () => {
        conn.off("DashboardUpdated", handler);
      };
    };
    
    // Try immediately
    const cleanup = checkAndSubscribe();
    
    // Also retry after a delay in case connection isn't ready yet
    const timer = setTimeout(() => {
      if (!cleanup) checkAndSubscribe();
    }, 2000);
    
    return () => {
      cleanup?.();
      clearTimeout(timer);
    };
  }, [queryClient]);
}