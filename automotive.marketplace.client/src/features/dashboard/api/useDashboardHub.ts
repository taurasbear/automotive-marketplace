import { chatConnectionRef } from "@/features/chat/api/useChatHub";
import { useQueryClient } from "@tanstack/react-query";
import { useEffect } from "react";

export function useDashboardHub() {
  const queryClient = useQueryClient();

  useEffect(() => {
    let unregister: (() => void) | undefined;

    const trySubscribe = () => {
      const conn = chatConnectionRef.current;
      if (!conn) return false;

      const handler = () => {
        void queryClient.invalidateQueries({ queryKey: ["dashboard-summary"] });
      };

      conn.on("DashboardUpdated", handler);
      unregister = () => conn.off("DashboardUpdated", handler);
      return true;
    };

    if (!trySubscribe()) {
      const timer = setTimeout(trySubscribe, 2000);
      return () => {
        clearTimeout(timer);
        unregister?.();
      };
    }

    return () => {
      unregister?.();
    };
  }, [queryClient]);
}