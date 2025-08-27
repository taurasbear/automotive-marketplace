import Login from "@/app/pages/Login";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/login")({
  component: Login,
});
