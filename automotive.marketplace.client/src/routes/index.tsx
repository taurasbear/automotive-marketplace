import MainPageContainer from "@/containers/MainPageContainer";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/")({
  component: MainPageContainer,
});