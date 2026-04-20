import { createFileRoute } from "@tanstack/react-router";
import Makes from "../pages/Makes";

export const Route = createFileRoute("/makes")({
  component: Makes,
});
