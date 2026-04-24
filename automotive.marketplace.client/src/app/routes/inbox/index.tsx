import Inbox from "@/app/pages/Inbox";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/inbox/")({
  component: Inbox,
});
