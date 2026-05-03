import { ListingSearch } from "@/features/search";
import { Dashboard } from "@/features/dashboard";
import { useAppSelector } from "@/hooks/redux";

const MainPage = () => {
  const userId = useAppSelector((state) => state.auth.userId);

  return (
    <div className="flex flex-1 flex-col">
      <ListingSearch className="mt-16 w-full sm:mt-64" />
      {userId && <Dashboard />}
    </div>
  );
};

export default MainPage;
