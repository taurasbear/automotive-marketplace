import Inbox from "@/app/pages/Inbox";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/inbox/$conversationId")({
  component: ConversationRoute,
});

function ConversationRoute() {
  const { conversationId } = Route.useParams();
  return <Inbox initialConversationId={conversationId} />;
}
