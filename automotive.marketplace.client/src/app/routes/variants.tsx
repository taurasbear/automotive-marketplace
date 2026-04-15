import { createFileRoute } from "@tanstack/react-router";
import Variants from "../pages/Variants";

export const Route = createFileRoute("/variants")({
  component: Variants,
});
