import { createFileRoute, redirect } from "@tanstack/react-router";
import Compare from "../pages/Compare";

const UUID_REGEX =
  /^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$/i;

export const Route = createFileRoute("/compare")({
  validateSearch: (search) => {
    const a = search["a"];
    const b = search["b"];
    if (
      typeof a !== "string" ||
      typeof b !== "string" ||
      !UUID_REGEX.test(a) ||
      !UUID_REGEX.test(b)
    ) {
      throw redirect({ to: "/" });
    }
    return { a, b };
  },
  component: Compare,
});
