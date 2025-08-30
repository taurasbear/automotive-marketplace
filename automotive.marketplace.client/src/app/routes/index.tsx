import MainPage from "@/app/pages/MainPage";
import { getAllMakesOptions } from "@/features/search/api/getAllMakesOptions";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  component: MainPage,
  loader: async ({ context: { queryClient } }) => {
    queryClient.prefetchQuery({ ...getAllMakesOptions, retry: false });
  },
});
