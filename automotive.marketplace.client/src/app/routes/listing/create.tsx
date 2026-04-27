import CreateListing from "@/app/pages/CreateListing";
import { createFileRoute, redirect } from "@tanstack/react-router";
import { store } from "@/lib/redux/store";

export const Route = createFileRoute("/listing/create")({
  beforeLoad: () => {
    const { auth } = store.getState();
    if (!auth.userId) {
      // eslint-disable-next-line @typescript-eslint/only-throw-error
      throw redirect({ to: "/login" });
    }
  },
  component: CreateListing,
});
