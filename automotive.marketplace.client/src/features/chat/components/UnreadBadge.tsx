import { selectAccessToken } from "@/features/auth";
import { useAppSelector } from "@/hooks/redux";
import { useQuery } from "@tanstack/react-query";
import { getUnreadCountOptions } from "../api/getUnreadCountOptions";

const UnreadBadge = () => {
  const accessToken = useAppSelector(selectAccessToken);
  const { data: unreadQuery } = useQuery({
    ...getUnreadCountOptions(),
    enabled: !!accessToken,
  });
  const count = unreadQuery?.data.unreadCount ?? 0;
  if (count === 0) return null;
  return (
    <span className="absolute top-1 -right-1 flex h-4 w-4 items-center justify-center rounded-full bg-red-500 text-[10px] font-bold text-white">
      {count > 99 ? "99+" : count}
    </span>
  );
};

export default UnreadBadge;
