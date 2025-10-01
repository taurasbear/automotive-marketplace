import { ListingSearch } from "@/features/search";
import { PiGithubLogo, PiLinkedinLogo } from "react-icons/pi";

const MainPage = () => {
  return (
    <div className="flex flex-col">
      <ListingSearch className="mt-16 w-full sm:mt-64" />
      <div className="mt-24 flex space-x-8 self-center">
        <a href="https://github.com/taurasbear/automotive-marketplace">
          <PiGithubLogo className="mx-auto size-10" /> Project Repo
        </a>

        <a href="https://www.linkedin.com/in/tauras-narvilas/">
          <PiLinkedinLogo className="mx-auto size-10" /> LinkedIn
        </a>
      </div>
    </div>
  );
};

export default MainPage;
