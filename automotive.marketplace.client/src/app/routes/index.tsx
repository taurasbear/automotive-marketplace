import MainPage from "@/app/pages/MainPage";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  component: MainPage,
});
