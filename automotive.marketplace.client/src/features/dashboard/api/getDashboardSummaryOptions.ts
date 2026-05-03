import { queryOptions } from "@tanstack/react-query";
import axiosClient from "@/lib/axios/axiosClient";
import type { GetDashboardSummaryResponse } from "../types/GetDashboardSummaryResponse";

export const getDashboardSummaryOptions = queryOptions({
  queryKey: ["dashboard-summary"],
  queryFn: async () => {
    const { data } = await axiosClient.get<GetDashboardSummaryResponse>(
      "/api/dashboard/summary",
    );
    return data;
  },
});