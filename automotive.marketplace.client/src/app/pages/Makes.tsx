import { CreateMakeDialog, MakeListTable } from "@/features/makeList";

const Makes = () => {
  return (
    <div className="grid items-center justify-center space-y-5 pt-10">
      <div className="flex w-full justify-end">
        <CreateMakeDialog />
      </div>
      <MakeListTable className="max-w-3xl" />
    </div>
  );
};

export default Makes;
