import Inbox from "@/app/pages/Inbox";
import { createFileRoute } from "@tanstack/react-router";

export const Route = createFileRoute("/inbox/$conversationId")({
  component: () => {
    const { conversationId } = Route.useParams();
    return <Inbox initialConversationId={conversationId} />;
  },
});
