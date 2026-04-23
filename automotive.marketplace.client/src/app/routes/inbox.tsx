import { Outlet, createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/inbox")({
  component: InboxLayout,
});

function InboxLayout() {
  return <Outlet />;
}
