import { useAppSelector } from "@/hooks/redux";
import { selectUnreadCount } from "../state/chatSlice";

const UnreadBadge = () => {
  const count = useAppSelector(selectUnreadCount);
  if (count === 0) return null;
  return (
    <span className="absolute -top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
      {count > 99 ? "99+" : count}
    </span>
  );
};

export default UnreadBadge;
