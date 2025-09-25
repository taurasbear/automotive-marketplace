import { createFileRoute } from "@tanstack/react-router";
import Cars from "../pages/Cars";

export const Route = createFileRoute("/cars")({
  component: Cars,
});
