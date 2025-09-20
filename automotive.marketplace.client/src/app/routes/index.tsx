import { getAllMakesOptions } from "@/api/make/getAllMakesOptions";
import MainPage from "@/app/pages/MainPage";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  component: MainPage,
  loader: ({ context: { queryClient } }) => {
    void queryClient.prefetchQuery({ ...getAllMakesOptions, retry: false });
  },
});
