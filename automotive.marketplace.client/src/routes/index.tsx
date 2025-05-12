import MainPage from "@/containers/MainPage";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  component: MainPage,
});